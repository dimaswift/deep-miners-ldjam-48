using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DeepMiners.Data
{
    [MaterialProperty("_BaseColor", MaterialPropertyFormat.Float4)]
    public struct BlockColor : IComponentData
    {
        public float4 Value;
    }
}