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

}
