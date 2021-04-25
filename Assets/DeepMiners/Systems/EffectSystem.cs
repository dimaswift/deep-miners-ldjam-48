using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class EffectSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            commandBufferSystem = World.GetExistingSystem<EntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            float delta = Time.DeltaTime;

            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            Entities.ForEach((Entity entity, ref Scale scale, ref BlockColor color, ref Effect effect) =>
            {
                effect.Timer += delta;

                if (effect.Timer >= effect.Duration)
                {
                    buffer.DestroyEntity(entity);
                }

                float4 c = color.Value;
                c.w = math.remap(0, effect.Duration, 0.5f, 0, effect.Timer);
                color.Value = c;

            }).Schedule();
        }
    }
}