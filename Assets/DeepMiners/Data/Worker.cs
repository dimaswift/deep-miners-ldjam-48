using Unity.Entities;

namespace DeepMiners.Data
{
    public struct Worker : IComponentData
    {
        public Entity CurrentBlock;
        public WorkerType Type;
        public float LastDentTime;
    }
}