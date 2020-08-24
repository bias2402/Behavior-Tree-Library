using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class BehaviorTreeObject : ScriptableObject {
    [Header("Execution Information")]
    public List<Node> nodes;

    [Header("TreeMaker Information")]
    public List<TreeNode> treeNodes;
    public List<NodeConnection> nodeConnections;
}
