using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct DestinationPoint : IComponentData
    {
        public int2 Value;
        public int2 PreviousPoint;
    }
}