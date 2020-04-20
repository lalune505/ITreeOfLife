using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct NodeViewJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float> nodesRads;
    [WriteOnly]
    public NativeArray<int> nodeIsLarge;
    
    public float camPosY;
    public float yMax;
    
    public void Execute(int i)
    {
        if (nodesRads[i] < math.lerp(0.0001f, 1f, camPosY / yMax))
        {
            nodeIsLarge[i] = 1;
        }
        else
        {
            nodeIsLarge[i] = 0;
        }
    }
}
