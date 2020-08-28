using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TreeHandler : MonoBehaviour {
    [SerializeField] private TreeObject tree = null;
    [Tooltip("This delay is in seconds!")]
    [SerializeField] private float executionDelay = 0;
    [Space]
    [Tooltip("Toggle this to log the tree execution to the console. This will spam the console A LOT and should be toggled off in any release!")]
    [SerializeField] private bool debugExecution = false;

    [Header("Leaf Actions")]
    [HideInInspector] [SerializeReference] private List<Node> nodes = new List<Node>();
    [SerializeField] [ReadOnly] private LeafMethod[] leafMethods = null;

    private float timeSinceLastExecution = 0;
    private List<Node> leaves = new List<Node>();

    private void OnValidate() {
        if (tree != null) {
            leafMethods = new LeafMethod[tree.leafCount];
            int index = 0;
            foreach(Node n in tree.nodes) {
                if (n.GetNodeType() == NodeTypes.Leaf) {
                    leafMethods[index] = new LeafMethod(n.GetNodeName());
                    index++;
                }
            }
        }

        if (executionDelay < 0) executionDelay = 0;
    }

    void Start() {
        nodes = new List<Node>(tree.nodes);
        foreach (Node n in nodes) {
            n.SetDebugMode(debugExecution);
            if (n.GetNodeType() == NodeTypes.Leaf) {
                n.SetLeafMethod(FindLeafMethod(n));
                leaves.Add(n);
            }
        }
    }

    LeafMethod FindLeafMethod(Node n) {
        foreach (LeafMethod m in leafMethods) {
            if (m.GetLeafName().Equals(n.GetNodeName())) {
                return m;
            }
        }
        return null;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (executionDelay > 0) {
                timeSinceLastExecution += Time.deltaTime;
                if (timeSinceLastExecution >= executionDelay) {
                    timeSinceLastExecution = 0;
                    nodes[0].Execute();
                }
            } else {
                nodes[0].Execute();
            }
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            LeafCallback(true);
        }
    }

    public void LeafCallback(bool result) {
        foreach (Node l in leaves) {
            if (l.IsNodeRunning()) {
                l.Callback(result);
                return;
            }
        }
        if (debugExecution) Debug.Log("No leaf is currently running");
    }
}

[Serializable]
public class LeafMethod {
    [SerializeField] [ReadOnly] private string leafName = "";
    [SerializeField] private UnityEvent leafMethod = null;

    public LeafMethod(string name) {
        leafName = name;
    }

    public void Execute() => leafMethod.Invoke();

    public string GetLeafName() {
        return leafName;
    }
}

public class ReadOnlyAttribute : PropertyAttribute {}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);                                  //Get the height of the object and its children (ture parameter adds children)
    }

    //This method stops the GUI from drawing, then draw the property fields using the attribute, and then it allows the GUI to draw again making the rest as it should be
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;                                                                        //Stop drawing the GUI
        EditorGUI.PropertyField(position, property, label, true);                                   //Write the property field including children
        GUI.enabled = true;                                                                         //Start drawing the GUI again
    }
}