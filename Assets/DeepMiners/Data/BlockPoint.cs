using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct BlockPoint : IComponentData
    {
        public int2 Value;
    }
}