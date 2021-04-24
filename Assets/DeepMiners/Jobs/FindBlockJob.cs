using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DeepMiners.Jobs
{
    [BurstCompile]
    public struct FindBlockJob : IJob
    {
        [ReadOnly] public int3 Point;
        [ReadOnly] public NativeHashMap<int3, Entity> Map;
        public NativeArray<Entity> Result;
        
        public void Execute()
        {
            Result[0] = Map[Point];
        }
    }
}