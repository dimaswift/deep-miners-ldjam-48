using Unity.Entities;

namespace DeepMiners.Data
{
    public struct DrillHit : IComponentData
    {
        public WorkerType WorkerType;
        public float Power;
    }
}