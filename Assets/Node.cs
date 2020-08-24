using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    private NodeTypes nodeType = NodeTypes.None;
    private Node parent = null;
    private List<Node> children = new List<Node>();
    private int currentChildRunning = 0;
    private bool isRunning = false;
    private string leafMethod = "";
    private TreeHandler handler = null;

    public void SetParent(Node parent) => this.parent = parent;

    public void AddChild(Node child) => children.Add(child);

    public void SetHandler(TreeHandler handler) => this.handler = handler;

    public void Execute() {
        if (nodeType != NodeTypes.Leaf) {
            children[currentChildRunning].Execute();
        } else {
            
        }
    }

    public void Callback(bool result) {
        switch (nodeType) {
            case NodeTypes.ConcurrenceSelecter:
                parent.Callback(result);
                break;
            case NodeTypes.Leaf:
                parent.Callback(result);
                break;
            case NodeTypes.PrioritySelector:
                parent.Callback(result);
                break;
            case NodeTypes.Sequence:
                parent.Callback(result);
                break;
            case NodeTypes.Inverter:
                parent.Callback(!result);
                break;
            case NodeTypes.Succeeder:
                parent.Callback(true);
                break;
        }
    }
}