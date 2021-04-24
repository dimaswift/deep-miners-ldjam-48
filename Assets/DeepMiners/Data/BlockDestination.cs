using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    [InternalBufferCapacity(32)]
    public struct BlockDestination : IBufferElementData
    {
        public static implicit operator int3(BlockDestination e) { return e.Value; }
        public static implicit operator BlockDestination(int3 e) { return new BlockDestination { Value = e }; }
        public int3 Value;
    }
}