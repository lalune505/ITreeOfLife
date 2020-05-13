using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct NodeView 
{
    public int nodeId;
    public string rank;
    public string sciName;
    public float nodeRad;
    public Vector3 pos;
    public Quaternion rot;
    public List<NodeView> childrenNodes;
    
    public void Init(Node node, float r,Vector3 position, Quaternion rotation, List<NodeView> children)
    {
        nodeId = node.id;
        rank = node.rank;
        sciName = node.sciName;
        
        nodeRad = r;
        pos = position;
        rot = rotation;
        childrenNodes = children;
    }

    public void AddChildrenNode(NodeView nodeView)
    {
        childrenNodes.Add(nodeView);
    }
    
    
}
