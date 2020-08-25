using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[Serializable]
public class TreeHandler : MonoBehaviour {
    [SerializeField] private TreeObject tree = null;

    [Header("Leaf Actions")]
    [HideInInspector] [SerializeReference] private List<Node> nodes = new List<Node>();
    [SerializeField] [ReadOnly] private LeafSetup[] leafEvents = null;

    private void OnValidate() {
        if (tree != null) {
            leafEvents = new LeafSetup[tree.leafCount];
            int index = 0;
            foreach(Node n in tree.nodes) {
                if (n.GetNodeType() == NodeTypes.Leaf) {
                    leafEvents[index] = new LeafSetup(n.GetNodeName());
                    index++;
                }
            }
        }
    }

    void Start() {
        Debug.Log(tree.treeNodes.Count);
        foreach (Node n in tree.nodes) {
            n.SetHandler(this);
        }
    }

    void Update() {
        
    }
}

[Serializable]
public class LeafSetup {
    [SerializeField] [ReadOnly] private string leafName = "";
    [SerializeField] private UnityEvent leafMethod = null;

    public LeafSetup(string name) {
        leafName = name;
    }
}

public class ReadOnlyAttribute : PropertyAttribute {

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}