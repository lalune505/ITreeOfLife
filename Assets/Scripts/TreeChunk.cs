using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeChunk
{
    public List<Vector3> nodes = new List<Vector3>();
    public List<int> sizes = new List<int>();
   
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

    public void Recalculate(List<NodeView> nodeViews)
    {
        foreach (var node in nodeViews)
        {
            nodes.Add(node.pos);
            sizes.Add(node.childrenNodes.Count);
            nodes.AddRange(node.childrenNodes.Select(x => x.pos));
        }
    }

    public void Dispose()
    {
        nodes.Clear();
        sizes.Clear();
    }
}
