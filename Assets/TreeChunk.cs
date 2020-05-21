using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeChunk {
    
    public readonly List<MeshData> meshData = new List<MeshData>();
    public Vector3 position;
    public int nodeId;
    public int chunkDepth;
    public float rad;
    
    public TreeChunk(Vector3 position, int id, int depth, float r)
    {
        this.position = position;
        nodeId = id;
        chunkDepth = depth;
        rad = r;
    }

    public void Recalculate(List<NodeView> nodes)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        var index = 0;
        var index1 = 0;

        foreach (var node in nodes)
        {
            vertices.Add(node.pos);
            foreach (var child in node.childrenNodes)
            {
                if (vertices.Count > 65000)
                {
                    meshData.Add(new MeshData(vertices.ToArray(), indices.ToArray()));
                    vertices.Clear();
                    indices.Clear();
                    vertices.Add(node.pos);
                    index = 0;
                    index1 = 0;
                }
                vertices.Add(child.pos);
                index++;
                indices.Add(index1);
                indices.Add(index);
            }
            index1 = index + 1;
            index = index1;
        }
        meshData.Add(new MeshData(vertices.ToArray(), indices.ToArray()));
        
        vertices.Clear();
        indices.Clear();
        
    }
    public void Dispose()
    {
        meshData.Clear();
    }
}
