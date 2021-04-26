using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct Mark : IComponentData
    {
        public float4 Color;
        public float Power;
        public float Duration;
        public Entity Block;

    }
}