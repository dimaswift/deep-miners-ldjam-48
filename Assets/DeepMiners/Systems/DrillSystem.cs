using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class DrillSystem : SystemBase
    {
        private BlockGroupSystem groupSystem;
        private EntityCommandBufferSystem commandBufferSystem;
        private DrillConfig config;

        protected override async void OnCreate()
        {
            groupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            config = await Addressables.LoadAssetAsync<DrillConfig>("configs/drill").Task;
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            if (groupSystem.IsReady == false)
            {
                return;
            }

            Dependency = groupSystem.WaitForModificationJob(Dependency);

            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            ComponentDataFromEntity<Dent> dentLookup = GetComponentDataFromEntity<Dent>();
            
            float time = (float)Time.ElapsedTime;

            float deltaTime = Time.DeltaTime;

            Dependency = Entities
                .ForEach((Entity entity,
                ref Worker worker, ref Translation translation, 
                ref NonUniformScale scale, 
                in DestinationPoint destination,
                in DrillPower power,
                in VerticalLimit verticalLimit,
                in DrillAnimations animations) =>
            {
                float drillTime = math.clamp(math.remap(worker.LastDentTime, worker.LastDentTime + power.Rate, 0, 1, time), 0, 1);

                float y = math.lerp(translation.Value.y,
                    verticalLimit.Value + CurveUtil.Evaluate(ref animations.Bounce.Value.Keyframes, drillTime),
                    deltaTime * 10);
                
                if (worker.CurrentBlock != Entity.Null && power.Amount > 0)
                {
                    translation.Value = new float3(translation.Value.x, y, translation.Value.z);
                }

                if (time - worker.LastDentTime > power.Rate)
                {
                    worker.LastDentTime = time;
                    if (worker.CurrentBlock != Entity.Null)
                    {
                        if (dentLookup.HasComponent(worker.CurrentBlock))
                        {
                            Dent dent = dentLookup[worker.CurrentBlock];
                            dent.Value -= power.Amount;
                            buffer.SetComponent(worker.CurrentBlock, dent);
                            if (dent.Value <= Dent.DestroyThreshold)
                            {
                                worker.CurrentBlock = Entity.Null;
                                buffer.RemoveComponent<IgnoreGravity>(entity);
                                buffer.SetComponent(entity, new VerticalVelocity() { Value = 10 } );
                            }
                        }
                    }
                }

            }).Schedule(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}