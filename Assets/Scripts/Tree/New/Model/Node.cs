using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int id;
    public int parentId;
    public Node[] childrenNodes;

    public int GetSize()
    {
       /* if (this.childrenNodes.Length == 0)
        {
            return 0;
        }*/
        return 1 + this.childrenNodes.Sum(x => x.GetSize());
    }
}
