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
    
    public Chunk(Vector3 position, int id, int depth, float r)
    {
        this.position = position;
        nodeId = id;
        chunkDepth = depth;
        rad = r;
    }

    public void Recalculate(Node1[] nodes)
    {
        var index = 0;
        var index1 = 0;
        
        for (var i = 0; i < nodes.Length; i++)
        {
            verticies.Add(nodes[i].pos);
            for (var j = 0; j < nodes[i].childrenNodes.Count; j++)
            {
                if (verticies.Count == 65000)
                {
                    verticies.Add(nodes[i].pos);
                    index = 0;
                    index1 = 0;
                }
                verticies.Add( nodes[i].childrenNodes[j].pos);
                index++;
                indices.Add(index1);
                indices.Add(index);
            }
            index1 = index + 1;
            index = index1;
        }
    }
    public void Dispose()
    {
        verticies.Clear();
        indices.Clear();
    }

}
