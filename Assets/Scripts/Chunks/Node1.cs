using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public struct Node1
{
    public int id;
    public Vector3 pos;
    public Quaternion rot;
    public float r;
    public List<Node1> childrenNodes;
    private int _size;

    public int GetSize()
    {
        var sum = 0;
        for (var i = 0; id < childrenNodes.Count; i++)
        {
            sum += childrenNodes[i].GetSize();
        }

        _size = 1 + sum;
        
        return _size;
    }
}
