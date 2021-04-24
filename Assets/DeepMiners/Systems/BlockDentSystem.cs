using DeepMiners.Data;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class BlockDentSystem : SystemBase
    {
        private float blockSize;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            blockSize = World.GetOrCreateSystem<BlockGroupSystem>().BlockSize;
        }

        protected override void OnUpdate()
        {
            float size = blockSize;
            EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Entities.WithNone<DestroyBlock>().ForEach((Entity entity, ref Translation translation, ref NonUniformScale scale,
                in Dent dent, in BlockPoint point, in BlockGroupVisualOrigin origin) =>
            {
                float y = origin.Value.y - point.Value.y;
                translation.Value = new float3(translation.Value.x, y - (((1f - dent.Value) / 2) * size),
                    translation.Value.z);
                scale.Value = new float3(scale.Value.x, size * dent.Value, scale.Value.z);
                if (dent.Value < 0.1f)
                {
                    commandBuffer.AddComponent(entity, new DestroyBlock());
                }

            }).Schedule(); 
        }
    }
}