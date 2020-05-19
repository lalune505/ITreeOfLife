using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class ChunkGenerator<T> : MonoBehaviour  where T : struct, IJob 
{
    private World world;
    
    public ChunkFileLoader fileLoader;
    
    struct JobHolder
    {
        internal T job;
        internal JobHandle handle;
        internal float startTime;
    }
    
    private Dictionary<Vector3, JobHolder> _jobs = new Dictionary<Vector3, JobHolder>();

    public int JobsInQueue
    {
        get { return _jobs.Count; }
    }
    
    public int JobsCompleted { get; private set; }
    public float TimeSpentAverage { get; private set; }
    private float runningSum = 0f;
    
    public Chunk LoadChunkAt(Vector3 worldOrigin)
    {
        int size = world.chunkSize;
        float scale = world.voxelSize;
        
        int buffer = size + 1;

        if (!_jobs.ContainsKey(worldOrigin))
        {
            var job = CreateJob(worldOrigin);

            var handle = job.Schedule();
            
            //Check to see if we completed it
            if (handle.IsCompleted)
            {
                handle.Complete();

                var sample = ChunkFromJob(job);
                
                var c = new Chunk();
            
                //c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(worldOrigin);

                return c;
            }
            
            var holder = new JobHolder()
            {
                job = job,
                handle = handle,
                startTime = Time.time
            };
            
            _jobs.Add(worldOrigin, holder);
        }
        else
        {
            var holder = _jobs[worldOrigin];

            if (holder.handle.IsCompleted)
            {
                holder.handle.Complete();

                var sample = ChunkFromJob(holder.job);
                
                var c = new Chunk();
            
               // c.Recalculate(size, scale, worldOrigin, true);
                
                if (fileLoader != null)
                    fileLoader.SaveChunk(c);

                _jobs.Remove(worldOrigin);

                float duration = Time.time - holder.startTime;

                runningSum += duration;

                JobsCompleted++;

                TimeSpentAverage = runningSum / JobsCompleted;

                return c;
            }
        }

        return null;
    }

    private T CreateJob(Vector3 worldOrigin)
    {
        return new T();
        
    }

    private float[] ChunkFromJob(T job)
    {
        return new float[]{};
        
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
