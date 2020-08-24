using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHandler : MonoBehaviour {
    [SerializeField] private TreeObject tree = null;

    void Start() {
        Debug.Log(tree.treeNodes.Count);
        foreach (Node n in tree.nodes) {
            n.SetHandler(this);
        }
    }

    void Update() {
        
    }
}