using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Tree", menuName = "ScriptableObjects/TreeObject", order = 1)]
public class TreeObject : ScriptableObject {
    public List<Node> nodes = new List<Node>();
    public List<TreeNode> treeNodes = new List<TreeNode>();
    public List<NodeConnection> nodeConnections = new List<NodeConnection>();
    public float something;
}