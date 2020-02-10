using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int id;
    public string rank;
    public NodeName names;
    public List<Node> childrenNodes = new List<Node>();

    public int GetSize()
    {
        return this.childrenNodes.Count == 0 ? 1 : this.childrenNodes.Sum(x => x.GetSize());
    }
}
