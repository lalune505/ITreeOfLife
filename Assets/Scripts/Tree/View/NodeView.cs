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
    
    private int _size;

    public int GetSize()
    {
        if (_size == 0)
        {
            _size = 1 + this.childrenNodes.Sum(x => x.GetSize());
        }
        return _size;
    }
    
    public void Init(float r,Vector3 position, Quaternion rotation)
    {
        nodeRad = r;
        pos = position;
        rot = rotation;
    }

    public void AddChildrenNode(NodeView nodeView)
    {
        childrenNodes.Add(nodeView);
    }
    
    public Matrix4x4 GetMatrix4X4()
    {
        return Matrix4x4.TRS(this.pos,  this.rot,
            new Vector3(this.nodeRad, this.nodeRad, this.nodeRad));
    }

}
