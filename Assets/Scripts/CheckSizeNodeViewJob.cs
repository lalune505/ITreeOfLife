using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CheckSizeNodeViewJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float> nodesRads;
    [ReadOnly] 
    public NativeArray<float3> positions;
    [WriteOnly]
    public NativeArray<int> nodesSizes;
    
    public float t;

    public void Execute(int i)
    {
        var p = positions[i];
        var r = nodesRads[i];
       
        if (r > math.lerp(0.0001f, 1f, 0.4 * t) & (0 < p.x & p.x < 1) & (0 < p.y & p.y < 1))
        {
            nodesSizes[i] = 1;
        }else
        {
            nodesSizes[i] = 0;
        }
    }
}
