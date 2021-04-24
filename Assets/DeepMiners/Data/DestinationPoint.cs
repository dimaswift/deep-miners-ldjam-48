using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct DestinationPoint : IComponentData
    {
        public int3 Value;
    }
}