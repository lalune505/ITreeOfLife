using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    private Node node;
    public int nodeId;
    public float branchLength;
    public string rank;
    public string sciName;
    
    public void Init(Node node)
    {
        this.node = node;
        this.nodeId = node.id;
        this.rank = node.rank;
        this.sciName = node.sciName;

    }
}
