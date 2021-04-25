using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct Worker : IComponentData
    {
        public Entity CurrentBlock;
        public WorkerAbility Ability;
        public float LastHitTime;
        public float SizeLossPerHit;
        public int Radius;
        public int MaxBounces;
        public float4 Color;
    }
}