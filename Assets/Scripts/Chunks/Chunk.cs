using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk 
{
    public readonly List<Vector3> verticies = new List<Vector3>();
    public readonly List<int> indices = new List<int>();
    public Vector3 position;
    public int nodeId;
    public int chunkDepth;
    public float rad;
    
    public Chunk(Vector3 position, Vector3[] verts, int id, int depth, float r)
    {
        this.position = position;
        nodeId = id;
        chunkDepth = depth;
        rad = r;
        verticies = verts.ToList();
    }
    public void Dispose()
    {
        verticies.Clear();
        indices.Clear();
    }

}
