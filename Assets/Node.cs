using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Node {
    [Header("Basic Node Settings")]
    private string nodeName = "";
    private NodeTypes nodeType = NodeTypes.None;
    private Node parent = null;
    private bool isRunning = false;
    [SerializeReference] private List<Node> children = new List<Node>();
    private int currentChildRunning = 0;
    private LeafMethod leafMethod = null;

    private bool isDebugging = false;

    public Node(string name, NodeTypes nodeType) {
        nodeName = name;
        this.nodeType = nodeType;
    }

    public NodeTypes GetNodeType() {
        return nodeType;
    }

    public string GetNodeName() {
        return nodeName;
    }

    public void SetParent(Node parent) => this.parent = parent;

    public void AddChild(Node child) => children.Add(child);

    public void SetLeafMethod(LeafMethod leafMethod) => this.leafMethod = leafMethod;

    public void SetDebugMode(bool isDebugging) => this.isDebugging = isDebugging;

    public bool IsNodeRunning() {
        return isRunning;
    }

    public void Execute() {
        if (isDebugging) {
            DebugMeSenpai(GetNodeName() + " is executing");
        }
        Debug.Log(GetNodeType());

        isRunning = true;
        switch (nodeType) {
            case NodeTypes.Leaf:
                leafMethod.Execute();
                break;
            case NodeTypes.Selector:
            case NodeTypes.Sequence:
                CheckIfAChildIsRunning();
                children[currentChildRunning].Execute();
                break;
            case NodeTypes.Inverter:
            case NodeTypes.Succeeder:
                children[currentChildRunning].Execute();
                break;
        }
    }

    public void Callback(bool result) {
        if (isDebugging) {
            if (GetNodeType() == NodeTypes.Leaf) {
                DebugMeSenpai(GetNodeName() + " has finished its execution with a result of " + result);
            } else {
                DebugMeSenpai(children[currentChildRunning].GetNodeName() + " has called back to its parent " + GetNodeName() + " with a result of " + result);
            }
        }

        isRunning = false;
        switch (nodeType) {
            case NodeTypes.Leaf:
                parent.Callback(result);
                break;
            case NodeTypes.Selector:
                currentChildRunning++;
                if (result || currentChildRunning >= children.Count) {
                    if (parent != null) parent.Callback(result);
                    else DebugMeSenpai(GetNodeName() + " is the top of the tree. Execution done for this frame.");
                } else Execute();
                break;
            case NodeTypes.Sequence:
                currentChildRunning++;
                if (result && currentChildRunning < children.Count) Execute(); 
                else {
                    currentChildRunning = 0;
                    if (parent != null) parent.Callback(result);
                    else DebugMeSenpai(GetNodeName() + " is the top of the tree. Execution done for this frame.");
                }
                break;
            case NodeTypes.Inverter:
                parent.Callback(!result);
                break;
            case NodeTypes.Succeeder:
                parent.Callback(true);
                break;
        }
    }

    public void CheckIfAChildIsRunning() {
        if (children.Count == 0 || children == null) {
            Debug.LogError(GetNodeName() + " does not have any children and it is not a leaf!");
            return;
        }

        for (int i = 0; i < children.Count; i++) {
            if (children[i].IsNodeRunning()) {
                currentChildRunning = i;
                return;
            }
        }
        currentChildRunning = 0;
    }

    void DebugMeSenpai(string debugLine) {
        Debug.Log(debugLine);
    }
}