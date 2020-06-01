using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CheckVisibleNodeViewJob : IJobParallelFor
{
    [ReadOnly] [DeallocateOnJobCompletion]
    public NativeArray<float3> positions;
    [WriteOnly]
    public NativeArray<int> visibleNodes;

    public void Execute(int index)
    {
        var p = positions[index];
        if ((0 < p.x & p.x < 1) /*& (0 < p.y  & p.y  < 1)*/)
        {
            visibleNodes[index] = 1;
        }
        else
        {
            visibleNodes[index] = 0;
        }
    }
}
