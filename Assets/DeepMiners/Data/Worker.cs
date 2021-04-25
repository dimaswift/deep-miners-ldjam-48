using Unity.Entities;

namespace DeepMiners.Data
{
    public struct Worker : IComponentData
    {
        public Entity CurrentBlock;
        public WorkerType Type;
        public float LastHitTime;
        public float SizeLossPerHit;
        public int Radius;
        public int MaxConsecutiveHits;
    }
}