using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public World world;
    public ChunkFileLoader fileLoader;
    
    struct JobHolder
    {
        internal ChunkJob job;
        internal JobHandle handle;
        internal float startTime;
    }
    
    private Dictionary<int, JobHolder> _jobs = new Dictionary<int, JobHolder>();

    public int JobsInQueue
    {
        get { return _jobs.Count; }
    }
    
    public int JobsCompleted { get; private set; }
    public float TimeSpentAverage { get; private set; }
    private float runningSum = 0f;
    
    public Chunk LoadChunkAt(Vector3 origin, int nodeId)
    {

        if (!_jobs.ContainsKey(nodeId))
        {
            var job = CreateJob(origin, nodeId);

            var handle = job.Schedule();
            
            //Check to see if we completed it
            if (handle.IsCompleted)
            {
                handle.Complete();

                var sample = ChunkFromJob(job);
                
                var c = new Chunk(origin,sample, nodeId, world.treeDepth, world.R);
            
                //c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(nodeId);

                return c;
            }
            
            var holder = new JobHolder()
            {
                job = job,
                handle = handle,
                startTime = Time.time
            };
            
            _jobs.Add(nodeId, holder);
        }
        else
        {
            var holder = _jobs[nodeId];

            if (holder.handle.IsCompleted)
            {
                holder.handle.Complete();

                var sample = ChunkFromJob(holder.job);
                
                var c = new Chunk(origin, sample,nodeId, world.treeDepth, world.R);
            
               // c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(nodeId);

                float duration = Time.time - holder.startTime;

                runningSum += duration;

                JobsCompleted++;

                TimeSpentAverage = runningSum / JobsCompleted;

                return c;
            }
        }

        return null;
    }

    private ChunkJob CreateJob(Vector3 origin, int nodeId)
    {

        return new ChunkJob
        {
            chunk = new NativeList<Vector3>(NodesDataFileCreator.nodes1[nodeId].GetSize(), Allocator.Persistent),
            rootNode = NodesDataFileCreator.nodes1[nodeId],
            origin = origin,
            chunkDepth = world.treeDepth,
            rad = world.R
        };
    }

    private Vector3[] ChunkFromJob(ChunkJob job)
    {
        Vector3[] array = job.chunk.ToArray();
        job.chunk.Dispose();
        return array;
    }

    private void OnDestroy()
    {
        foreach (var key in _jobs.Keys)
        {
            var holder = _jobs[key];
            
            holder.handle.Complete(); //Complete the job before we die

            ChunkFromJob(holder.job); //Ensure we clear the chunk data
        }
    }

}
public struct ChunkJob : IJob
{
    public Node1 rootNode;
    public int chunkDepth;
    public float rad;
    public Vector3 origin;
    
    public NativeList<Vector3> chunk;
    public void Execute()
    {
        CreateSubTree(rootNode, chunkDepth, rad, origin, Quaternion.identity);
    }
    private void CreateSubTree(Node1 node, int depth,float r, Vector3 pos, Quaternion rot)
    {
        node.pos = pos;
        node.r = r;
        node.rot = rot;
        
        chunk.Add(node.pos);
        if (depth == 0) return;
        float sumAngle = 0f;

        Matrix4x4 m = GetMatrix4X4(node);
        float sum = GetSqrtSum(node);

        for (var i = 0; i < node.childrenNodes.Length; i++)
        {
            float childAngle = GetNodeAngle(node, sum);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, 1 - childRad);
            sumAngle += childAngle;
            
            CreateSubTree(node.childrenNodes[i],depth - 1, childRad * r, m.MultiplyPoint3x4(childNodePos), Quaternion.LookRotation(Vector3.forward,m.MultiplyVector(childNodePos)));
        }
    }

    private static Matrix4x4 GetMatrix4X4(Node1 node)
    {
        return Matrix4x4.TRS(node.pos,  node.rot,
            new Vector3(node.r, node.r, node.r));
    }

    private static Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * math.cos((angle) * math.PI/180f);
        endPoint.y = branchLength * math.sin((angle) * math.PI/180f);
        endPoint.z = 0;
        return endPoint; 
    }

    private static float GetNodeAngle(Node1 node,float sum)
    {
        return 180f * math.sqrt(node.GetSize()) / sum;
    }

    private static float GetSqrtSum(Node1 node)
    {
        float sum = 0f;
        for (var i = 0; i < node.childrenNodes.Length; i++)
        {
           sum += math.sqrt(node.childrenNodes[i].GetSize());
        }

        return sum;
    }
    private static float GetNodeRadius(float nodeAngle)
    {
        float result;
        if (math.abs(nodeAngle - 90f) < 0.00001f)
        {
            result = 0.9f;
        }
        else
        {
            float t =  math.tan(nodeAngle  * math.PI/180f);
            result = t / (t + 1);
        }
        return result;
    }
}

