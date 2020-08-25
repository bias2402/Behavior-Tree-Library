//Developed by Tobias Oliver Jensen, Game Development & Learning Technology master student at the University of Southern Denmark, 2020
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum NodeTypes { ConcurrenceSelecter, Inverter, Leaf, None, PrioritySelector, Sequence, Succeeder }
public class TreeMaker : EditorWindow {
	public Texture concurrenceSelector = null;
	public Texture inverter = null;
	public Texture leaf = null;
	public Texture prioritySelector = null;
	public Texture sequence = null;
	public Texture succeeder = null;
	public Texture delete = null;

	private List<TreeNode> treeNodes = new List<TreeNode>();
	private List<NodeConnection> nodeConnections = new List<NodeConnection>();
	private Rect dropTargetRect = new Rect(10.0f, 10.0f, 30.0f, 30.0f);
	private float[] rectSize = { 80, 30 };
	public NodeConnection currentConnection { get; internal set;} = null;
	public Vector3 mousePos { get; internal set; } = Vector2.zero;

	private bool draggingAll = false;
	private Vector2 draggingAllStart = Vector2.zero;
	private Vector2 draggingAllCurrent = Vector2.zero;
	private TreeNode root = null;

	[Header("Node informations")]
	private TreeNode selectedNode = null;
	private string nodeName = "";
	private NodeTypes nodeType = NodeTypes.PrioritySelector;

	[MenuItem("Window/Tree Maker")]
	public static void Launch() {
		GetWindow(typeof(TreeMaker)).Show();
	}

	void OnInspectorUpdate() {
		Repaint();
	}

	public void PassNodeInformation(string nodeName, NodeTypes nodeType) {
		this.nodeName = nodeName;
		this.nodeType = nodeType;
	}

	void NodeEditPanel() {
		Rect topRect = new Rect(0, 0, 200, position.height);
		GUILayout.BeginArea(topRect, GUI.skin.GetStyle("Box"));
		if (GUILayout.Button("Reset Root Position")) {
			treeNodes[0].Position = new Vector2(position.width / 2, 50);
		}
		if (GUILayout.Button("Add Node")) {                                                                     //If the button is clicked, add a new object to the window and the list
			treeNodes.Add(new TreeNode("Node " + (treeNodes.Count), rectSize, new Vector2(210, 50), this));
		}
		EditorGUI.BeginChangeCheck();
		GUILayout.Label("Node name:");
		nodeName = EditorGUILayout.TextField(nodeName);
		GUILayout.Label("Node type:");
		nodeType = (NodeTypes)EditorGUILayout.EnumPopup(nodeType);
		if (EditorGUI.EndChangeCheck()) {
			selectedNode.SetName(nodeName);
			selectedNode.SetNodeType(nodeType);
		}
		GUILayout.EndArea();

		Rect bottomRect = new Rect(0, position.height - 30, 200, position.height);
		GUILayout.BeginArea(bottomRect, GUI.skin.GetStyle("Box"));
		if (GUILayout.Button("Create Tree Object")) {
			CreateBehaviorTreeObject();
		}
		GUILayout.EndArea();
	}

	public void OnGUI() {
		if (root == null) {
			treeNodes.Add(root = new TreeNode("Root", rectSize, new Vector2(position.width / 2, 50), this));
			treeNodes[0].MakeRoot();
		}
		wantsMouseMove = true;
		NodeEditPanel();																						//Draw the edit panel
		TreeNode toFront, dropDead;
		bool previousState;
		Color color;

		toFront = dropDead = null;

        //Drag and drop, recoloring, selecting, and stacking for all nodes
        #region
        foreach (TreeNode n in treeNodes) {                                                          //Go through all the spawned objects
			previousState = n.Dragging;																		//Save bool value about data being dragged last time it was checked

			color = GUI.color;                                                                              //Save the default color
			if (n.isSelected) {                                                                             //Check if the node is selected
				GUI.color = Color.yellow;                                                                       //Make n yellow
			} else if (n.isRoot && n.Dragging) {
				GUI.color = Color.grey;
			} else {
				GUI.color = color;																				//Make n standard GUI color
			}
			n.OnGUI();																						//Call OnGUI on the object
			GUI.color = color;																				//Change the color of the GUI back to the original

			if (n.Dragging) {																				//If data is being dragged
				if (!n.isRoot) {
					n.isSelected = true;
					selectedNode = n;
					n.ParseNodeInfoToEditPanel();
					foreach (TreeNode otherNodes in treeNodes) {                                                        //Go through all nodes	
						if (otherNodes != n) {																			
							otherNodes.isSelected = false;
							GUI.color = color;                                                                              //Set color of all but this node to standard GUI color
						}
					}
				}
				if (treeNodes.IndexOf(n) != treeNodes.Count - 1) {											//If the object isn't the last in the list
					toFront = n;																						//Set reference for toFront to equal data
				}
			} else if (previousState) {																			//If data was previously dragged
				if (dropTargetRect.Contains(Event.current.mousePosition)) {											//If the mouse is on the object
					dropDead = n;																					//Set dropDead reference to equal data
				}
			}
		}

		if (toFront != null) {																				//Move an object to front by removing and adding it
			treeNodes.Remove(toFront);
			treeNodes.Add(toFront);
		}

		if (dropDead != null) {																				//Destroy an object by removing it
			treeNodes.Remove(dropDead); 
		}
		#endregion

		try {
			foreach (NodeConnection nc in nodeConnections) {                                                    //Draw all connections
				nc.OnGUI();
			}
		} catch {}

		//Event checks
		#region
		if (Event.current.type == EventType.ScrollWheel) {													//Check if mouse wheel event
			if (Event.current.delta.y < 0) {
				Zoom(1);
			} else {
				Zoom(-1);
			}
			Event.current.Use();
		} else if (Event.current.type == EventType.MouseDown) {												//Check for mouse down event
			if (Event.current.button == 0) {
				bool isHoveringConnection = false;
				foreach (NodeConnection nc in nodeConnections) {
					if (nc.connectionRect.Contains(Event.current.mousePosition)) {
						isHoveringConnection = true;
						break;
					}
				}
				if (!isHoveringConnection) {
					draggingAll = true;                                                                                 //Set to true
					draggingAllStart = Event.current.mousePosition;                                                     //Save current mouse position as start position
					draggingAllCurrent = Event.current.mousePosition;                                                   //Save current mouse position
					foreach (TreeNode to in treeNodes) {                                                                    //Go through all tree objects
						to.SetDraggingAll(true);                                                                            //Call SetDraggingAll with parameter true
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
			foreach (TreeNode to in treeNodes) {																	//Go through all tree objects
				to.SetDraggingAll(false);																			//Call SetDraggingAll with parameter false
			}
			Event.current.Use();                                                                                //Trigger the event to use it
		} else if (Event.current.type == EventType.MouseDrag) {												//Check for mouse drag (mouse down + mouse move) event
			draggingAllCurrent = Event.current.mousePosition;													//Update with current mouse position
			Event.current.Use();                                                                                //Trigger the event to use it
		} else if (Event.current.type == EventType.MouseMove) {
			mousePos = Event.current.mousePosition;
		}
		if (draggingAll) {																					//If draggingAll is true
			foreach (TreeNode to in treeNodes) {															//Go through all tree objects
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
			rectSize[0] = 80;																					//Set to 80
			rectSize[1] = 30;                                                                                   //Set to 30
		} else if (direction == -1) {																		//If scroll down
			rectSize[0] = 130;                                                                                  //Set to 130
			rectSize[1] = 80;																					//Set to 80
		}	
		foreach(TreeNode to in treeNodes) {                                                             //Go through all tree objects
			to.ChangeSize(rectSize);                                                                            //Call ChangeSize with parameter rectSize
		}
	}

	public void AddNodeConnection(NodeConnection nodeConnection) => nodeConnections.Add(nodeConnection);

	public void SetCurrentConnection(NodeConnection nodeConnection) => currentConnection = nodeConnection;

	public void RemoveConnection(int index) {
		if (nodeConnections.Count - 1 <= index) nodeConnections.RemoveAt(index);
	}

	public void FindImproperConnections(TreeNode nodeToCheckAgainst, bool isParent) {
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
		Debug.Log("Instantiate");
		TreeObject behaviorTree = CreateInstance<TreeObject>();
		behaviorTree.name = "Tree1";
		behaviorTree.nodeConnections = nodeConnections;
		behaviorTree.treeNodes = treeNodes;
		behaviorTree.something = 1;
		foreach (TreeNode treeNode in treeNodes) {
			Node n = new Node();
		}
		AssetDatabase.CreateAsset(behaviorTree, "Assets/Trees/" + behaviorTree.name + ".asset");
		AssetDatabase.SaveAssets();
	}
}

public class GUIDraggableObject {
	protected Vector2 pos;
	public Vector2 dragStart;
	private bool isDragging;
	private bool isDraggingAll = false;
	private Vector2 draggingAllStart = Vector2.zero;

	public GUIDraggableObject(Vector2 position) {
		pos = position;
	}

	public bool Dragging {																									//Get Dragging
		get {
			return isDragging;
		}
	}

	public Vector2 Position {																								//Get or Set Position
		get {
			return pos;
		}
		set {
			pos = value;
		}
	}

	public void Drag(Rect draggingRect) {																					
		if (Event.current.type == EventType.MouseUp) {																		//If mouse (left) is up
			isDragging = false;																									//Not dragging
		} else if (Event.current.type == EventType.MouseDown && draggingRect.Contains(Event.current.mousePosition)) {		//If mouse (left) is down and inside object
			isDragging = true;																									//Is dragging
			dragStart = Event.current.mousePosition - pos;																		//Set dragStart to current mouse position minus object position
			Event.current.Use();																								//Trigger the event
		}

		if (isDragging && !isDraggingAll) {																					//If only this object is being dragged
			Vector2 newPos = Event.current.mousePosition - dragStart;															//Update position to current mouse position minus start position
			//newPos.x += SnapAxisValue(newPos.x);
			//newPos.y += SnapAxisValue(newPos.y);
			//Debug.Log(newPos);
			pos = newPos;
		}
	}

	//Snap using mouse pos. When mouse is more than 2.5 away, move and update dragStart for new snap start point
	float SnapAxisValue(float value) {
		int offset = (Mathf.Round(value) % 5) >= 3 ? 5 : 0;
		return (value % 5) * offset; 
	}

	public void SetDraggingAll(bool dragAll) {
		isDraggingAll = dragAll;																			//Set to the value of dragAll
		if (dragAll) draggingAllStart = Position;															//If dragAll is true, set current position as start position
	}

	public void DraggingAllSetPosition(Vector2 newPos) {
		Position = draggingAllStart - newPos;																//Set position to start position minus the recieved delta movement for mouse
	}
}

[Serializable]
public class TreeNode : GUIDraggableObject {
	private TreeMaker treeMaker = null;
	public bool isRoot { get; internal set; } = false;
	public string name { get; internal set; }
	private float[] size;
	public Vector3 childConnectionPoint = Vector3.zero;
	public Vector3 parentConnectionPoint = Vector3.zero;
	private TreeNode parent = null;
	[SerializeReference] private List<TreeNode> children = new List<TreeNode>(); 
	public bool isSelected = false;
	private NodeTypes nodeType = NodeTypes.PrioritySelector;

	public TreeNode(string name, float[] size, Vector2 position, TreeMaker treeMaker) : base(position) {
		this.name = name;
		this.size = size;
		this.treeMaker = treeMaker;
	}

	public void OnGUI() {
		Rect drawRect = new Rect(pos.x, pos.y, size[0], size[1]);													//Draw a rect

		GUILayout.BeginArea(drawRect, GUI.skin.GetStyle("Box"));													//Begin GUILayout block
		GUILayout.Label(name, GUI.skin.GetStyle("Box"), GUILayout.ExpandWidth(true));                               //Make a label with the Node's name
		GUILayout.EndArea();                                                                                        //End GUILayout block

		Rect setParentButton = new Rect(drawRect.width / 2, 0, 10, 10);												//Make the rect for the parentButton
		Rect addChildButton = new Rect(drawRect.width / 2, drawRect.height, 10, 10);								//Make the rect for the childButton

		childConnectionPoint = drawRect.position + addChildButton.position;											//Calculate the position for the children to connect to
		parentConnectionPoint = drawRect.position + setParentButton.position;                                       //Calculate the position for the parent to connect to

		//This region controls the drawing of the node
        #region
        GUILayout.BeginArea(new Rect(pos.x - 5, pos.y - 5, size[0] + 10, size[1] + 10));							//Create a new box around the node that is a bit larger
		if (!isRoot) {																								//Check if node is the root node
			//This button is for setting the parent node of the current connection aka setting this node as a child! It's confusing, I know.
			if (GUI.Button(setParentButton, "")) {																			//Add a new button for connecting a parent to the node
				if (treeMaker.currentConnection == null) {                                                                  //Check if a connection is currently acive
					if (parent == null) {																						//Check that node doesn't have a parent yet
						new NodeConnection(this, null, treeMaker);                                                                  //Create a new connection, setting this node as the child
					} else {
						Debug.LogWarning("A node can only ever have one parent!");
					}
				} else {
					if (!treeMaker.currentConnection.GotChild()) {																//Check if the current connection have a child node
						if (parent == null) {                                                                                       //Check if the node's parent is null
							if (treeMaker.currentConnection.GetParent() != this) {                                                      //Check that the current connection's parent node isn't this node
								treeMaker.currentConnection.FinishConnection(this);                                                     //Finish the connection
							} else {
								Debug.LogWarning("A node can't connect to itself!");
							}
						} else {
							Debug.LogWarning("A node can only ever have one parent!");
						}
					} else {
						Debug.LogWarning("The connection already got a parent node set!");
					}
				}
			}
		}
		if (nodeType != NodeTypes.Leaf) {
			//This button is for setting the child node of the current connection aka setting this node as a parent! It's confusing, I know.
			if (GUI.Button(addChildButton, "")) {                                                                           //Add a new button for connecting a child to the node
				if ((nodeType == NodeTypes.Inverter && children.Count >= 1) || (nodeType == NodeTypes.Succeeder && children.Count >= 1)) {
					Debug.LogWarning("A decorater can only have one child!");
				} else {
					if (treeMaker.currentConnection == null) {                                                                  //Check if a connection is currently acive
						new NodeConnection(null, this, treeMaker);                                                                  //Create a new connection, setting this node as the parent
					} else {
						if (!treeMaker.currentConnection.GotParent()) {                                                             //Check if the current connections have a parent node
							if (treeMaker.currentConnection.GetChild() != this) {                                                       //Check that the current connection's child node isn't this node
								treeMaker.currentConnection.FinishConnection(this);                                                         //Finish the connection
							} else {
								Debug.LogWarning("A node can't connect to itself!");
							}
						} else {
							Debug.LogWarning("The connection already got a child node set! " + treeMaker.currentConnection.GetChild());
							Debug.LogWarning(children.Count);
						}
					}
				}
			}
		}
		if (size[0] > 100) {
			switch (nodeType) {
				case NodeTypes.ConcurrenceSelecter:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.concurrenceSelector);
					break;
				case NodeTypes.Inverter:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.inverter);
					break;
				case NodeTypes.Leaf:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.leaf);
					break;
				case NodeTypes.PrioritySelector:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.prioritySelector);
					break;
				case NodeTypes.Sequence:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.sequence);
					break;
				case NodeTypes.Succeeder:
					GUI.DrawTexture(new Rect(drawRect.width / 2 - 20, 25, 50, 50), treeMaker.succeeder);
					break;
			}
		}
		GUILayout.EndArea();
        #endregion

        if (nodeType == NodeTypes.Inverter || nodeType == NodeTypes.Succeeder) {
			if (children.Count > 1) {
				int iterations = children.Count;
				Debug.Log("Child count: " + children.Count);
				for (int i = 1; i < iterations; i++) {
					Debug.Log("Ran"); 
					treeMaker.FindImproperConnections(this, true);
					if (children.Count > 1) children.RemoveAt(1);
				}
			}
		}
		if (nodeType == NodeTypes.Leaf && children.Count != 0) {
			children.Clear();
			treeMaker.FindImproperConnections(this, true);
		}
		Drag(drawRect);																								//Drag the rect
	}

	public void MakeRoot() => isRoot = true;

	public void ChangeSize(float[] newSize) => size = newSize;

	public void SetName(string name) {
		this.name = name;
		OnGUI();
	}

	public void SetParent(TreeNode parent) => this.parent = parent;

	public void AddChild(TreeNode child) => children.Add(child);

	public void RemoveChild(TreeNode child) {
		int index = -1;
		for (int i = 0; i < children.Count; i++) {
			if (children[i] == child) {
				index = i;
				break;
			}
		}
		if (index == -1) return;
		children.RemoveAt(index);
		Debug.Log("Removed a child");
	}

	public TreeNode GetParent() {
		return parent;
	}

	public void SetNodeType(NodeTypes type) => nodeType = type;

	public void ParseNodeInfoToEditPanel() {
		treeMaker.PassNodeInformation(name, nodeType);
	}
}

[Serializable]
public class NodeConnection {
	public Rect connectionRect { get; internal set; }
	private TreeMaker treeMaker = null;
	private TreeNode childNode = null;
	private TreeNode parentNode = null;
	private Vector3 childPoint = Vector3.zero;
	private Vector3 parentPoint = Vector3.zero;
	private bool isDeleting = false;

	public NodeConnection(TreeNode child, TreeNode parent, TreeMaker treeMaker) {
		if (child == null && parent == null) {
			Debug.LogError("No start nor end node was given! Connection not created!");
			return;
		}
		if (child != null) {
			childNode = child;
			childPoint = child.parentConnectionPoint;
		}
		if (parent != null) {
			parentNode = parent;
			parentPoint = parent.childConnectionPoint;
		}
		treeMaker.SetCurrentConnection(this);
		treeMaker.AddNodeConnection(this);
		this.treeMaker = treeMaker;
	}

	public void OnGUI() {
		if (isDeleting) return;
		if (childNode == null && parentNode == null) {
			Debug.LogError("No start nor end node!");
			return;
		}
		childPoint = childNode != null ? childNode.parentConnectionPoint : treeMaker.mousePos;
		parentPoint = parentNode != null ? parentNode.childConnectionPoint : treeMaker.mousePos;
		Handles.DrawLine(childPoint, parentPoint);

		if (parentNode != null && childNode != null) {
			Vector2 size = new Vector2(15, 15);
			Vector2 pos = ((parentPoint + childPoint) / 2) - new Vector3(size.x / 2, size.y / 2, 0);
			Rect drawRect = new Rect(pos, size);
			connectionRect = drawRect;
			GUILayout.BeginArea(drawRect);
			Rect btnRect = new Rect(0, 0, drawRect.width, drawRect.height);
			if (GUI.Button(btnRect, treeMaker.delete)) {
				DeleteConnection(false);
			}
			GUILayout.EndArea();
		}
	}

	public bool IsFullyConnected() {
		return childNode == null || parentNode == null ? false : true;
	}

	public void FinishConnection(TreeNode n) {
		if (parentNode == null) {
			parentNode = n;
			Debug.Log(treeMaker.currentConnection);
			childNode.SetParent(parentNode);
			parentNode.AddChild(childNode);
		} else {
			childNode = n;
			parentNode.AddChild(childNode);
			childNode.SetParent(parentNode);
		}
		treeMaker.SetCurrentConnection(null);
	}

	public bool GotChild() {
		return childNode == null ? false : true;
	}

	public bool GotParent() {
		return parentNode == null ? false : true;
	}

	public TreeNode GetChild() {
		return childNode;
	}

	public TreeNode GetParent() {
		return parentNode;
	}

	public void DeleteConnection(bool isPhantom) {
		isDeleting = true;
		if (childNode != null) childNode.SetParent(null);
		if (parentNode != null) parentNode.RemoveChild(childNode);
		childNode = null;
		parentNode = null;
		if (!isPhantom) treeMaker.RemoveConnection(treeMaker.GetConnectionIndex(this));
	}
}