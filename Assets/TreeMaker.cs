//Developed by Tobias Oliver Jensen, Game Development & Learning Technology master student at the University of Southern Denmark, 2020
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum NodeTypes { Inverter, Leaf, None, Selector, Sequence, Succeeder }
public class TreeMaker : EditorWindow {
	public Texture inverter = null;
	public Texture leaf = null;
	public Texture prioritySelector = null;
	public Texture root = null;
	public Texture sequence = null;
	public Texture succeeder = null; 
	public Texture delete = null;

	private List<Node> nodes = new List<Node>();
	private List<NodeConnection> nodeConnections = new List<NodeConnection>();
	private Rect dropTargetRect = new Rect(10.0f, 10.0f, 30.0f, 30.0f);
	private float[] rectSize = { 80, 30 };
	public NodeConnection currentConnection { get; internal set;} = null;
	public Vector3 mousePos { get; internal set; } = Vector2.zero;

	private bool draggingAll = false;
	private Vector2 draggingAllStart = Vector2.zero;
	private Vector2 draggingAllCurrent = Vector2.zero;
	private Node treeRoot = null;

	[Header("Node informations")]
	private Node selectedNode = null;
	private string nodeName = "";
	private NodeTypes nodeType = NodeTypes.Selector;

	[Header("Tree Options")]
	private string treeName = "";
	private string error = "";
	private bool isTreeNameInUse = false;
	private TreeObject loadedTree = null;
	private bool isTreeLoaded = false;

	[MenuItem("Window/Tree Maker")]
	public static void Launch() {
		GetWindow(typeof(TreeMaker)).Show();
	}

	void OnInspectorUpdate() {
		Repaint();																							//This repaints the UI to update changes
	}

	public void PassNodeInformation(string nodeName, NodeTypes nodeType) {
		this.nodeName = nodeName;
		this.nodeType = nodeType;
	}

	void NodeEditPanel() {
		Rect topRect = new Rect(0, 0, 200, position.height);												//Define the rect for this window
		GUILayout.BeginArea(topRect, GUI.skin.GetStyle("Box"));												//This is used to make an area that can be drawn by the editor GUI drawer
		if (GUILayout.Button("Reset Root Position")) {														//If the button is clicked, reset the root node's position
			nodes[0].pos = new Vector2(position.width / 2, 50);
		}
		if (GUILayout.Button("Add Node")) {                                                                 //If the button is clicked, add a new object to the window and the list
			nodes.Add(new Node("Node " + (nodes.Count), rectSize, new Vector2(210, 50), this));
		}
		EditorGUI.BeginChangeCheck();																		//This is used to check for updates in the text fields
		GUILayout.Label("Node name:");																		
		nodeName = EditorGUILayout.TextField(nodeName);														//Read and set the string during edits
		GUILayout.Label("Node type:");
		nodeType = (NodeTypes)EditorGUILayout.EnumPopup(nodeType);											//Make a dropdown based on the NodeTypes enum
		if (EditorGUI.EndChangeCheck()) {																	//If changes happened, update the information in the currently selected node
			selectedNode.SetName(nodeName);
			selectedNode.SetNodeType(nodeType);
		}
		GUILayout.EndArea();                                                                                //Tell the editor to stop drawing here

		Rect bottomRect = new Rect();																		//New rect that is at the bottom of the previous
		if (loadedTree == null) {
			bottomRect = new Rect(0, position.height - 120, 200, position.height);
		} else {
			bottomRect = new Rect(0, position.height - 60, 200, position.height);
		}
		GUILayout.BeginArea(bottomRect, GUI.skin.GetStyle("Box"));
		loadedTree = (TreeObject)EditorGUILayout.ObjectField(loadedTree, typeof(TreeObject), true);			//Draw an objectfield to pass a ScriptableObject of type TreeObject
		if (loadedTree == null) {                                                                           //If a tree object hasn't been loaded, draw the UI for making one
			if (isTreeLoaded) {
				isTreeLoaded = false;
			}
			GUILayout.Label("Tree name:");
			treeName = EditorGUILayout.TextField(treeName);
			error = "";
			isTreeNameInUse = false;
			foreach (string guid in AssetDatabase.FindAssets(treeName, new[] { "Assets/Trees" })) {             //Go through all assets at the given location (the string)
				string temp = AssetDatabase.GUIDToAssetPath(guid);                                                  //Translate from GUID to string
				temp = temp.Substring(13, temp.Length - 13);                                                        //Hardcoded substring
				temp = temp.Substring(0, temp.Length - 6);                                                          //Hardcoded substring
																													//The final substring is the asset's name without location and extension

				if (treeName.Equals(temp)) {                                                                        //Check if the currently entered name equals the asset's name												
					error = "Name already in use!";
					isTreeNameInUse = true;
					break;
				}
			}
			GUILayout.Label(error);
			if (GUILayout.Button("Create Tree Object")) {                                                       //If the button is clicked, call the method to create a new ScriptableObject asset
				CreateBehaviorTreeObject();
			}
		} else {																							//If a tree object has been loaded, draw the UI for saving and updating it
			if (!isTreeLoaded) {
				if (isTreeNameInUse) {
					SaveTreeChanges();
				}
				isTreeLoaded = true;
				nodes.Clear();
				foreach (Node n in loadedTree.nodes) {																//Foreach node in the tree object, create a new node for the tree maker
					nodes.Add(new Node(n.GetNodeName(), rectSize, n.pos, this));
				}

				for (int i = 0; i < loadedTree.nodes.Count; i++) {
					nodes[i].ParseLoadedInformation(loadedTree.nodes[i].GetNodeName(), loadedTree.nodes[i].GetNodeType(), 
						loadedTree.nodes[i].size, loadedTree.nodes[i].isRoot, this);                                    //Parse information about the tree object node to the new tree maker node
					List<Node> children = loadedTree.nodes[i].GetChildren();
					foreach (Node n in children) {																		//Go through the tree object node's children
						nodes[i].AddChild(nodes[n.listIndex]);																//Add child to the new node with the same index as the tree object node
					}
					if (loadedTree.nodes[i].GetParent() != null) {
						nodes[i].SetParent(nodes[loadedTree.nodes[i].GetParent().listIndex]);                               //Set parent for the new node to the index of the tree object node's parent
					}
				}

				nodeConnections.Clear();
				for (int i = 0; i < loadedTree.nodeConnections.Count; i++) {                                        //Foreach connection in the tree object, create a new connection for the tree maker
					new NodeConnection(nodes[loadedTree.nodeConnections[i].GetChild().listIndex],
						nodes[loadedTree.nodeConnections[i].GetParent().listIndex], this, true);
				}

				if (currentConnection != null) currentConnection.DeleteConnection(true);							//Ensure that current connection is purged
			}
			if (GUILayout.Button("Save changes to Tree")) {														//If the button is clicked, call the method to save changes to the tree object
				SaveTreeChanges();
			}
		}
		GUILayout.EndArea();
	}

	public void OnGUI() {
		if (treeRoot == null) {
			nodes.Add(treeRoot = new Node("Root", rectSize, new Vector2(position.width / 2, 50), this));
			nodes[0].MakeRoot();
			nodes[0].SetTypeToRoot();
		}
		wantsMouseMove = true;
		NodeEditPanel();																						//Draw the edit panel
		Node toFront, dropDead;
		bool previousState;
		Color color;

		toFront = dropDead = null;

        //Drag and drop, recoloring, selecting, and stacking for all nodes
        #region
        foreach (Node n in nodes) {																		//Go through all the spawned objects
			previousState = n.isDragging;																	//Save bool value about data being dragged last time it was checked

			color = GUI.color;                                                                              //Save the default color
			if (n.isSelected) {                                                                             //Check if the node is selected
				GUI.color = Color.yellow;                                                                       //Make n yellow
			} else if (n.isRoot && n.isDragging) {
				GUI.color = Color.grey;																			//Make n grey
			} else {
				GUI.color = color;																				//Make n standard GUI color
			}
			n.OnGUI();																						//Call OnGUI on the object
			GUI.color = color;																				//Change the color of the GUI back to the original

			if (n.isDragging) {																				//If n is being dragged
				if (!n.isRoot) {
					n.isSelected = true;
					selectedNode = n;	
					n.ParseNodeInfoToEditPanel();															//Parse the node's information to the edit panel in the GUI
					foreach (Node otherNodes in nodes) {                                                        //Go through all nodes	
						if (otherNodes != n) {																			
							otherNodes.isSelected = false;
							GUI.color = color;                                                                              //Set color of all but this node to standard GUI color
						}
					}
				}
				if (nodes.IndexOf(n) != nodes.Count - 1) {														//If the object isn't the last in the list
					toFront = n;																						//Set reference for toFront to equal data
				}
			} else if (previousState) {																		//If n was previously dragged
				if (dropTargetRect.Contains(Event.current.mousePosition)) {										//If the mouse is on the object
					dropDead = n;																					//Set dropDead reference to equal data
				}
			}
		}

		if (toFront != null) {																			//Move an object to front by removing and adding it
			nodes.Remove(toFront);
			nodes.Add(toFront);
		}

		if (dropDead != null) {																			//Destroy an object by removing it
			nodes.Remove(dropDead); 
		}
		#endregion
		try {
			foreach (NodeConnection nc in nodeConnections) {                                                //Draw all connections
				nc.OnGUI();
			}
		} catch {}

		//Event checks
		#region
		if (Event.current.type == EventType.ScrollWheel) {												//Check if mouse wheel event
			if (Event.current.delta.y < 0) {
				Zoom(1);
			} else {
				Zoom(-1);
			}
			Event.current.Use();
		} else if (Event.current.type == EventType.MouseDown) {											//Check for mouse down event
			if (Event.current.button == 0) {
				bool isHoveringConnection = false;
				foreach (NodeConnection nc in nodeConnections) {
					if (nc.connectionRect.Contains(Event.current.mousePosition)) {
						isHoveringConnection = true;
						break;
					}
				}
				if (!isHoveringConnection) {
					draggingAll = true;																				//Set to true
					draggingAllStart = Event.current.mousePosition;                                                 //Save current mouse position as start position
					draggingAllCurrent = Event.current.mousePosition;                                               //Save current mouse position
					foreach (Node to in nodes) {                                                                    //Go through all tree objects
						to.SetDraggingAll(true);                                                                        //Call SetDraggingAll with parameter true
					}
				}
			} else if (Event.current.button == 1) {
				if (currentConnection != null) {
					currentConnection.DeleteConnection(false);
					currentConnection = null;
				}
			}
			Event.current.Use();																				//Trigger the event to use it
		} else if (Event.current.type == EventType.MouseUp) {												//Check for mouse up event
			draggingAll = false;																				//Set to false
			foreach (Node to in nodes) {																		//Go through all tree objects
				to.SetDraggingAll(false);																			//Call SetDraggingAll with parameter false
			}
			Event.current.Use();                                                                                //Trigger the event to use it
		} else if (Event.current.type == EventType.MouseDrag) {												//Check for mouse drag (mouse down + mouse move) event
			draggingAllCurrent = Event.current.mousePosition;													//Update with current mouse position
			Event.current.Use();                                                                                //Trigger the event to use it
		} else if (Event.current.type == EventType.MouseMove) {												//Check if mouse moved
			mousePos = Event.current.mousePosition;																//Update the mousePos based on mouse position
		}
		if (draggingAll) {																					//If draggingAll is true
			foreach (Node to in nodes) {																		//Go through all tree objects
				to.DraggingAllSetPosition(draggingAllStart - draggingAllCurrent);									//Call DraggingAllSetPosition with parameter of the calculated delta for mouse movement
			}
		}
		#endregion

		NodeEditPanel();                                                                                    //Redraw to put panel on-top of all other elements

		if (currentConnection == null) {
			int index = -1;
			for (int i = 0; i < nodeConnections.Count; i++) {
				if (!nodeConnections[i].GotParent() || !nodeConnections[i].GotChild()) {
					index = i;
					break;
				}
			}
			if (index != -1) RemoveConnection(index);
		} else {
			if (currentConnection.GotChild() && currentConnection.GotParent()) {
				Debug.LogWarning("A phantom connection! BE GONE!");
				currentConnection.DeleteConnection(true);
				currentConnection = null; 
			}
		}
	}

    public void Zoom(int direction) {
		if (direction == 1) {																				//If scroll up
			rectSize[0] = 80;																					//Set width 80
			rectSize[1] = 30;                                                                                   //Set height 30
		} else if (direction == -1) {																		//If scroll down
			rectSize[0] = 130;                                                                                  //Set width 130
			rectSize[1] = 80;                                                                                   //Set height 80
		}

		foreach (Node tn in nodes) {                                                             //Go through all tree objects
			tn.ChangeSize(rectSize);                                                                            //Call ChangeSize with parameter rectSize
		}
	}

	public void AddNodeConnection(NodeConnection nodeConnection) => nodeConnections.Add(nodeConnection);

	public void SetCurrentConnection(NodeConnection nodeConnection) => currentConnection = nodeConnection;

	public void RemoveConnection(int index) {
		if (nodeConnections.Count - 1 <= index) nodeConnections.RemoveAt(index);
	}

	public void FindImproperConnections(Node nodeToCheckAgainst, bool isParent) {
		int connectionToRemove = -1;
		nodeConnections.ForEach(n => { if (n.GetParent() == nodeToCheckAgainst) connectionToRemove = GetConnectionIndex(n); });
		Debug.Log(nodeConnections.Count);
		if (connectionToRemove != -1) {
			nodeConnections[connectionToRemove].DeleteConnection(false);
		}
	}

	public int GetConnectionIndex(NodeConnection nodeConnection) {
		for (int i = 0; i < nodeConnections.Count; i++) {
			if (nodeConnections[i] == nodeConnection) {
				return i;
			}
		}
		return 0;
	}

	void CreateBehaviorTreeObject() {
		if (isTreeNameInUse) {																				
			Debug.LogError("Name already in use!");
			return;
		}
		if (treeName.Equals(string.Empty)) {
			Debug.LogError("No name entered!");
			return;
		}
		TreeObject behaviorTree = CreateInstance<TreeObject>();												//Create a new instance of the scriptable object TreeObject
		behaviorTree.name = treeName;																		//Set the object's name
		behaviorTree.nodeConnections = nodeConnections;														//Add the current node connections to the object

		List<Node> rootedNodes = new List<Node>();
		int index = 0;
		foreach (Node n in nodes) {																			//Go through all the nodes 
			if (n.IsConnectedToRoot()) {																		//Ensure that the node is connected to the root (otherwise we don't want it in the final tree)
				rootedNodes.Add(n);																				
				n.listIndex = index;
				index++;
			}
		}
		behaviorTree.nodes = rootedNodes;																	//Add the viable nodes to the object

		int leafCount = 0;
		foreach (Node n in rootedNodes) {																	//Go through all viable nodes and find the number of leaves (leaf nodes)
			if (n.GetNodeType() == NodeTypes.Leaf) {
				leafCount++;
			}
		}
		behaviorTree.leafCount = leafCount;																	//Add the leafCount to the object

		AssetDatabase.CreateAsset(behaviorTree, "Assets/Trees/" + behaviorTree.name + ".asset");			//Create a asset in the folder with the user-entered name
		AssetDatabase.SaveAssets();																			//Save assets to write the new asset to the disk
	}

	void SaveTreeChanges() {
		isTreeLoaded = false;
		Debug.Log("Here");
	}
}
 