using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "ScriptableObjects/SceneDataScriptableObject", order = 2)]
public class SceneData : ScriptableObject
{
    [SerializeField]
    public List<NodeView> nodeViews;
    
    [SerializeField]
    public List<MeshData> meshData;

}

[Serializable]
public struct MeshData
{
    public Vector3[] vertices;
    public int[] tris;

    public MeshData(Vector3[] vertices, int[] tris)
    {
        this.vertices = vertices;
        this.tris = tris;
    }
}
