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
    [WriteOnly]
    public NativeArray<int> nodesSizes;
    
    public float t;

    public void Execute(int i)
    {
        if (nodesRads[i] > math.lerp(0.0001f, 1f, 0.4 * t))
        {
            nodesSizes[i] = 1;
        }else
        {
            nodesSizes[i] = 0;
        }
    }
}
