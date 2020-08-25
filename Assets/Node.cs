using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Node {
    private string nodeName = "";
    private NodeTypes nodeType = NodeTypes.None;
    private Node parent = null;
    [SerializeReference] private List<Node> children = new List<Node>();
    private int currentChildRunning = 0;
    private bool isRunning = false;
    private string leafMethod = "";
    private TreeHandler handler = null;

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

//public class OldNode {
//    public string nodeName = "";
//    public NodeType type;
//    public BehaviourTreeExecutor mng = null;
//    public List<OldNode> children = new List<OldNode>();
//    public OldNode parent = null;
//    public string methodToExecute = "";

//    public bool result = false;
//    public float delay = 0;
//    public int currentChildRunning = 0;
//    public bool isRunning = false;
//    public bool isDecorator = false;
//    public bool debugRun = false;

//    public OldNode(string nodeName) {
//        this.nodeName = nodeName;
//    }

//    public OldNode(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) {
//        this.nodeName = nodeName;
//        this.parent = parent;
//        this.mng = mng;
//        this.debugRun = debugRun;
//    }

//    public virtual void ParentCallback(bool childResult) {
//        if (debugRun) {
//            if (parent == null) {
//                Debug.Log(nodeName + " is the top of the tree. Call back done (" + Time.timeSinceLevelLoad + ")");
//            } else {
//                Debug.Log(nodeName + " calls back to " + parent.nodeName + " (" + Time.timeSinceLevelLoad + ")");
//            }
//        }
//        result = childResult;
//        isRunning = false;
//    }

//    public virtual void ExecuteAction() {
//        isRunning = true;
//    }

//    public void CheckIfAChildIsRunning() {
//        if (children.Count == 0 || children == null) {
//            return;
//        }

//        for (int i = 0; i < children.Count; i++) {
//            if (children[i].isRunning) {
//                currentChildRunning = i;
//                return;
//            }
//        }
//    }

//    public void DebugMeSenpai() {
//        Debug.Log("Name: " + nodeName + " children count: " + children.Count + " and my parent is: " + parent);
//    }

//    public virtual void DebugExecution(string debugLine) {
//        Debug.Log(debugLine);
//    }
//}


////Decorator//
//public class Inverter : OldNode {
//    public Inverter (string nodeName) : base (nodeName) { type = NodeType.Inverter; isDecorator = true; }
//    public Inverter(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) : base(nodeName, parent, mng, debugRun) { type = NodeType.Inverter; isDecorator = true; }

//    public override void ParentCallback(bool childResult) {
//        base.ParentCallback(childResult);
//        result = !childResult;
//        if (parent != null) {
//            parent.ParentCallback(result);
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " executing child " + children[currentChildRunning].nodeName + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        children[currentChildRunning].ExecuteAction();
//    }
//}


////Leaf//
//public class Leaf : OldNode {
//    public Leaf(string nodeName) : base(nodeName) { type = NodeType.Leaf; isDecorator = false; }
//    public Leaf(string nodeName, OldNode parent, BehaviourTreeExecutor mng, string method, bool debugRun) : base(nodeName, parent, mng, debugRun) {
//        methodToExecute = method;
//        type = NodeType.Leaf;
//    }

//    public override void ParentCallback(bool childResult) {
//        base.ParentCallback(childResult);
//        if (parent != null) {
//            parent.ParentCallback(result);
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " simulating the execution of method " + methodToExecute + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        mng.SetExecutingLeafReference(this);
//        mng.behaviourActions.Invoke(methodToExecute, 0);
//    }
//}


////Composite//
//public class SelectorConcurrence : OldNode {
//    public SelectorConcurrence(string nodeName) : base(nodeName) { type = NodeType.SelectorConcurrence; isDecorator = false; }
//    public SelectorConcurrence(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) : base(nodeName, parent, mng, debugRun) { type = NodeType.SelectorConcurrence; }

//    public override void ParentCallback(bool childResult) {
//        base.ParentCallback(childResult);
//        currentChildRunning++;
//        if (currentChildRunning == children.Count) {
//            if (parent != null) {
//                parent.ParentCallback(result);
//            }
//        } else {
//            ExecuteAction();
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " executing child " + children[currentChildRunning].nodeName + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        CheckIfAChildIsRunning();
//        children[currentChildRunning].ExecuteAction();
//    }
//}


////Composite//
//public class SelectorPriority : OldNode {
//    public SelectorPriority(string nodeName) : base(nodeName) { type = NodeType.SelectorPriority; isDecorator = false; }
//    public SelectorPriority(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) : base(nodeName, parent, mng, debugRun) { type = NodeType.SelectorPriority; }

//    public override void ParentCallback(bool childResult) {
//        base.ParentCallback(childResult);
//        currentChildRunning++;
//        if (result || currentChildRunning >= children.Count) {
//            if (parent != null) {
//                parent.ParentCallback(result);
//            }
//        } else {
//            ExecuteAction();
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " executing child " + children[currentChildRunning].nodeName + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        children[currentChildRunning].ExecuteAction();
//    }
//}


////Composite//
//public class Sequence : OldNode {
//    public Sequence(string nodeName) : base(nodeName) { type = NodeType.Sequence; isDecorator = false; }
//    public Sequence(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) : base(nodeName, parent, mng, debugRun) { type = NodeType.Sequence; }

//    public override void ParentCallback(bool childResult) {
//        base.ParentCallback(childResult);
//        currentChildRunning++;
//        if (result && currentChildRunning >= children.Count) {
//            ExecuteAction();
//        } else {
//            if (parent != null) {
//                parent.ParentCallback(result);
//            }
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " executing child " + children[currentChildRunning].nodeName + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        CheckIfAChildIsRunning();
//        children[currentChildRunning].ExecuteAction();
//    }
//}


////Decorator//
//public class Succeeder : OldNode {
//    public Succeeder(string nodeName) : base(nodeName) { type = NodeType.Succeeder; isDecorator = true; }
//    public Succeeder(string nodeName, OldNode parent, BehaviourTreeExecutor mng, bool debugRun) : base(nodeName, parent, mng, debugRun) { type = NodeType.Succeeder; isDecorator = true; }

//    public override void ParentCallback(bool childResult) {
//        result = true;
//        if (parent != null) {
//            parent.ParentCallback(result);
//        }
//    }

//    public override void ExecuteAction() {
//        if (debugRun) {
//            Debug.Log(nodeName + " executing child " + children[currentChildRunning].nodeName + " (" + Time.timeSinceLevelLoad + ")");
//        }
//        base.ExecuteAction();
//        children[currentChildRunning].ExecuteAction();
//    }
//}