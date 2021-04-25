using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class BlockDepthSystem : SystemBase
    { 
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref Translation translation,
                in Depth depth, in BlockPoint point) =>
            {
                translation.Value = new float3(translation.Value.x, -depth.Value, translation.Value.z);
            }).Schedule(); 
        }
    }
}