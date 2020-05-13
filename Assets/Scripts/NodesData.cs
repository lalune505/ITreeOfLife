using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NodesData", menuName = "ScriptableObjects/NodesDataScriptableObject", order = 1)]
public class NodesData : ScriptableObject
{
    [SerializeField]
    IntNodeDictionary nodes;

    public IDictionary<int, Node> IntNodeDictionary
    {
        get => nodes;
        set => nodes.CopyFrom(value);
    }
}