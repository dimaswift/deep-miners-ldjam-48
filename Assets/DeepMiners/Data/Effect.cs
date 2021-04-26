using Unity.Entities;

namespace DeepMiners.Data
{
    public struct Effect : IComponentData
    {
        public float Timer;
        public float Duration;
        public float Size;
        public Entity Block;
    }
}