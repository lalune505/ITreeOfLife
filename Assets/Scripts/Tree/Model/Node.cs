using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int id;
    public string rank;
    public string sciName;
    public string authority;
    public string commonName;
    public string synonym;
    public List<Node> childrenNodes = new List<Node>();

    public int GetSize()
    {
        return  1 + this.childrenNodes.Sum(x => x.GetSize());
    }
}
