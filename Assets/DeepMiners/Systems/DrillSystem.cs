using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    public class DrillSystem : SystemBase
    {
        private BlockGroupSystem groupSystem;
        private EntityCommandBufferSystem commandBufferSystem;
        private DrillConfig config;

        private Random random;
        
        protected override async void OnCreate()
        {
            random = Random.CreateFromIndex((uint)Time.ElapsedTime);
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

            var map = groupSystem.BlocksMap;
            
            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            ComponentDataFromEntity<Dent> dentLookup = GetComponentDataFromEntity<Dent>();
            
            float time = (float)Time.ElapsedTime;

            float deltaTime = Time.DeltaTime;
            var size = groupSystem.GroupSize;
            var depth = groupSystem.CurrentDepth;
            
            NativeHashMap<int3, int> checkMap = new
                NativeHashMap<int3, int>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);
                
            NativeList<int3> checkList = new
                NativeList<int3>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);


            var rand = random;
            var isDrilled = GetComponentDataFromEntity<IsBeingDrilling>();

            var origin = groupSystem.VisualOrigin;
            
            
            
            Dependency = Entities.WithReadOnly(isDrilled).WithReadOnly(map)
                .ForEach((Entity entity,
                ref Worker worker,
                ref Translation translation, 
                ref NonUniformScale scale, 
                ref DestinationPoint destination,
                ref VerticalLimit verticalLimit,
                in DrillPower power,
                in DrillAnimations animations) =>
            {
                float drillTime = math.clamp(math.remap(worker.LastDentTime, worker.LastDentTime + power.Rate, 0, 1, time), 0, 1);

                var visualDestination = origin + destination.Value;

                bool inRange = math.distancesq(new float2(visualDestination.x, visualDestination.z),
                    new float2(translation.Value.x, translation.Value.z)) < 0.01f;
                
                if (inRange)
                {
                    float y =
                        verticalLimit.Value + CurveUtil.Evaluate(ref animations.Bounce.Value.Keyframes, drillTime) * verticalLimit.FlightHeight;

                    if (worker.CurrentBlock != Entity.Null && power.Amount > 0)
                    {
                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, y, deltaTime * 100 * worker.Damping), translation.Value.z);
                        worker.Damping = math.lerp(worker.Damping, 1, deltaTime);
                    }
                    else
                    {
                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, verticalLimit.Value + verticalLimit.FlightHeight, deltaTime * 10), translation.Value.z);
                    }
                }
                else
                {
                    translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, verticalLimit.Value + verticalLimit.FlightHeight, deltaTime * 10),
                        translation.Value.z);
                }

                if (time - worker.LastDentTime > power.Rate && inRange)
                {
                    worker.LastDentTime = time;
                    if (worker.CurrentBlock != Entity.Null)
                    {
                        if (dentLookup.HasComponent(worker.CurrentBlock))
                        {
                            Dent dent = dentLookup[worker.CurrentBlock];
                            dent.Value -= power.Amount;
                            verticalLimit.Value = (-destination.Value.y + 1) - ((1f - dent.Value) / scale.Value.x);
                            buffer.SetComponent(worker.CurrentBlock, dent);
                            if (dent.Value <= Dent.DestroyThreshold)
                            {
                                worker.CurrentBlock = Entity.Null;
                            }
                        }
                        else
                        {
                            worker.CurrentBlock = Entity.Null;
                        }
                    }
                    else
                    {
                        if (BlockUtil.GetClosestBlockOnSameLevel(rand, new int3(destination.Value.x, destination.Value.y, destination.Value.z), isDrilled, map, checkMap, checkList, size,
                            out var closest))
                        {
                            verticalLimit.Value = -destination.Value.y + 1f / scale.Value.x;
                            buffer.AddComponent<IgnoreGravity>(entity);
                            worker.CurrentBlock = map[closest];
                            buffer.AddComponent(worker.CurrentBlock, new IsBeingDrilling()); 
                            worker.Damping = 0.25f;
                            destination.Value = closest;
                        }
                        else
                        {
                           
                            destination.Value.y++;
                            var newDest = destination.Value;
                            newDest.y--;
                            if (map[newDest] != Entity.Null)
                            {
                                destination.Value.y-=2;
                            }
                        }
                        
                    }
                }

            }).Schedule(Dependency);

            Dependency.Complete();
            
            checkList.Dispose(Dependency);
            checkMap.Dispose(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}