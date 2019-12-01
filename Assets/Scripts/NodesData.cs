using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New NodesData", menuName = "Nodes Data", order = 51)]
public class NodesData : ScriptableObject
{
    [SerializeField]
    public Dictionary<int, Node> nodes;
}
