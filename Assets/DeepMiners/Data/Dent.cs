using Unity.Entities;

namespace DeepMiners.Data
{
    public struct Dent : IComponentData
    {
        public float Value;
        public const float DestroyThreshold = 0.1f;
    }
}