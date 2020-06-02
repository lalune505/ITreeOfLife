using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct NodeView 
{
    public string sciName;
    public float nodeRad;
    public Vector3 pos;
    [NonSerialized]
    public Quaternion rot;
    public List<NodeView> childrenNodes;
    
    public void Init(Node node, float r,Vector3 position, Quaternion rotation, List<NodeView> children)
    {
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
