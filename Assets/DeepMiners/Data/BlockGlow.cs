using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DeepMiners.Data
{
    [MaterialProperty("_EmissionColor", MaterialPropertyFormat.Float4)]
    public struct BlockGlow : IComponentData
    {
        public float4 Value;
    }
}