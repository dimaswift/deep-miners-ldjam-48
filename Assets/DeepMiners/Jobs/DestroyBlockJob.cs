using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.Jobs
{
    [BurstCompile]
    public struct DestroyBlockJob : IJob
    {
        [ReadOnly] public int3 Point;
        public NativeHashMap<int3, Entity> Map;

        public void Execute()
        {
            Map[Point] = Entity.Null;
        }
    }
}