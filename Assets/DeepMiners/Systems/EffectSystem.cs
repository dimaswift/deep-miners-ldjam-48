using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class EffectSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;

        private EntityQuery blocksQuery;
        
        
        protected override void OnCreate()
        {
            blocksQuery = GetEntityQuery(typeof(Translation));
            commandBufferSystem = World.GetExistingSystem<EntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            float delta = Time.DeltaTime;

            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();

            var blocks = GetComponentDataFromEntity<Translation>(true);
            
            Entities.WithReadOnly(blocks).ForEach((Entity entity, ref Scale scale, ref BlockColor color, ref Effect effect) =>
            {
                effect.Timer += delta;

                Translation v = blocks[entity];
                
                v.Value = blocks[effect.Block].Value + new float3(0, 0.01f ,0);
                
                buffer.SetComponent(entity, v);
                
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