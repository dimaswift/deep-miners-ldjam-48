using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct Worker : IComponentData
    {
        public Entity CurrentBlock;
        public WorkerAbility Ability;
        public float Frequency;
        public float Timer;
        public float SizeLossPerHit;
        public int Radius;
        public float MarkDuration;
        public int MaxBounces;
        public float4 Color;
    }
}