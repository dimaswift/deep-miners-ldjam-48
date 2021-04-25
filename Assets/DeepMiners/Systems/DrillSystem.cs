using System;
using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

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

            var map = groupSystem.BlocksMap;
            
            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            ComponentDataFromEntity<Dent> dentLookup = GetComponentDataFromEntity<Dent>();
            
            float time = (float)Time.ElapsedTime;

            float deltaTime = Time.DeltaTime;
            var size = groupSystem.GroupSize;

            NativeHashMap<int2, int> checkMap = new
                NativeHashMap<int2, int>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);
                
            NativeList<int2> checkList = new
                NativeList<int2>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);


            var rand = Random.CreateFromIndex((uint)Time.ElapsedTime);;
            var isDrilled = GetComponentDataFromEntity<IsBeingDrilling>();

            var origin = groupSystem.VisualOrigin;

            float scaleMultiplierThreshold = 0.25f;
            
            Dependency = Entities.WithReadOnly(dentLookup)
                .ForEach((Entity entity,
                ref Worker worker,
                ref Translation translation, 
                ref NonUniformScale scale, 
                ref DestinationPoint destination,
                ref VerticalLimit verticalLimit,
                ref DrillPower power,
                in DrillAnimations animations) =>
                {
                    float scaleMultiplier = math.max(scale.Value.x, scaleMultiplierThreshold);
                    
                    float drillTime = math.clamp(math.remap(worker.LastDentTime, worker.LastDentTime + power.Rate * scaleMultiplier, 0, 1, time), 0, 1);
                    
                    float3 visualDestination = origin + new float3(destination.Value.x, translation.Value.y, destination.Value.y);

                    bool inRange = math.distancesq(new float2(visualDestination.x, visualDestination.z),
                        new float2(translation.Value.x, translation.Value.z)) < 0.01f;
                    
                    if (inRange)
                    {
                        float y =
                            verticalLimit.Value + CurveUtil.Evaluate(ref animations.Bounce.Value.Keyframes, drillTime) * verticalLimit.FlightHeight;

                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, y, deltaTime * 75), translation.Value.z);
                        worker.Damping = math.lerp(worker.Damping, 1, deltaTime);
                    }
                    else
                    {
                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, verticalLimit.Value + verticalLimit.FlightHeight, deltaTime * 10), translation.Value.z);
                    }

                    if (time - worker.LastDentTime > power.Rate * scaleMultiplier && inRange)
                    {
                        worker.LastDentTime = time;
                        float newScale = scale.Value.x - worker.SizeLossPerHit;
                        scale.Value = new float3(newScale, newScale, newScale);
                        Dent dent = dentLookup[worker.CurrentBlock];
                        dent.Value += power.Amount * scale.Value.x;
                        buffer.AddComponent(worker.CurrentBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                        var center = destination.Value;
                        
                        if (worker.Radius == 1)
                        {
                            var current = destination.Value;
   
                            current.x++;
                            if (current.x < size.x)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.x--;
                            if (current.x >= 0)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.y--;
                            if (current.y >= 0)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }
                            
                            current = center;
                            current.y++;
                            if (current.y < size.y)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }
                            
                            verticalLimit.Value = -dent.Value;
                            buffer.SetComponent(worker.CurrentBlock, dent);
                        }
                        
                        if (worker.Radius == 2)
                        {
                            var current = destination.Value;
   
                            current.x++;
                            if (current.x < size.x)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.x--;
                            if (current.x >= 0)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.y--;
                            if (current.y >= 0)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }
                            
                            current = center;
                            current.y++;
                            if (current.y < size.y)
                            {
                                var nextPower = power.Amount * rand.NextFloat(0.25f, 0.75f);
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            
                            
                            float spread = 0.15f;
                            
                            current = center;
                             current.x--;
                             current.y++;
                            if (current.x >= 0 && current.y < size.y)
                            {
                                var nextPower = power.Amount * spread;
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.x--;
                            current.y--;
                            if (current.x >= 0 && current.y >= 0)
                            {
                                var nextPower = power.Amount * spread;
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }

                            current = center;
                            current.y--;
                            current.x++;
                            if (current.y >= 0 && current.x < size.x)
                            {
                                var nextPower = power.Amount * spread;
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }
                            
                            current = center;
                            current.x++;
                            current.y++;
                            if (current.y < size.y && current.x < size.x)
                            {
                                var nextPower = power.Amount * spread;
                                var nextBlock = map[current];
                                var nextDent = dentLookup[nextBlock];
                                nextDent.Value += nextPower * scale.Value.x;
                                buffer.SetComponent(nextBlock, nextDent);
                                buffer.AddComponent(nextBlock, new DrillHit() { WorkerType = worker.Type, Power = power.Amount });
                            }
                        }
                        
                        verticalLimit.Value = -dent.Value;
                        buffer.SetComponent(worker.CurrentBlock, dent);
                        power.Hits++;
                        if (power.Hits > worker.MaxConsecutiveHits)
                        {
                            power.Hits = 0;
                            if (BlockUtil.GetClosestBlockOnSameLevel(rand, destination.Value, dentLookup, isDrilled, map, checkMap, checkList, size, out int2 next))
                            {
                                buffer.RemoveComponent<IsBeingDrilling>(worker.CurrentBlock); 
                                destination.Value = next;
                                worker.CurrentBlock = map[next];
                            }
                        }
                                
                        if (newScale < 0.1f)
                        {
                            buffer.DestroyEntity(entity);
                            buffer.RemoveComponent<IsBeingDrilling>(worker.CurrentBlock); 
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