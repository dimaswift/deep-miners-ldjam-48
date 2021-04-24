using Unity.Entities;
using Unity.Mathematics;

namespace DeepMiners.Data
{
    public struct VerticalLimit : IComponentData
    {
        public float Value;
        public float Bounciness;
    }
}