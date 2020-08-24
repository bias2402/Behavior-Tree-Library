using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    private Node parent = null;
    private List<Node> children = new List<Node>();

    public Node(Node parent) {
        this.parent = parent;
    }

    public void AddChild(Node child) => children.Add(child);
}
