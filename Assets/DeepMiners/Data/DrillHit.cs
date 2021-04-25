using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct DrillHit : IComponentData
    {
        public WorkerAbility WorkerAbility;
        public float4 Color;
        public float Power;
    }
}