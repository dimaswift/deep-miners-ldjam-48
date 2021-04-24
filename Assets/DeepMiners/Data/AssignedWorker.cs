using Unity.Entities;

namespace DeepMiners.Data
{
    public struct AssignedWorker : IComponentData
    {
        public Entity Worker;
    }
}