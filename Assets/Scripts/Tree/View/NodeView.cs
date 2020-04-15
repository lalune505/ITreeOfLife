using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    public int nodeId;
    public string rank;
    public string sciName;
    public int depth;
    public float nodeRad;
    public List<NodeView> childrenNodes = new List<NodeView>();
    
    public void Init(Node node)
    {
        this.nodeId = node.id;
        this.rank = node.rank;
        this.sciName = node.sciName;
    }

    public void AddChildrenNode(NodeView nodeView)
    {
        childrenNodes.Add(nodeView);
    }
    
    
}
