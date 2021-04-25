using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    public class DrillSystem : SystemBase
    {
        private BlockGroupSystem groupSystem;
        private EntityCommandBufferSystem commandBufferSystem;

        public JobHandle? CurrentJob;
        
        protected override void OnCreate()
        {
            groupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        private static void IncreaseDepth(int2 size, 
            int2 current,
            EntityCommandBuffer buffer, 
            float amount,
            NativeHashMap<int2, Entity> map,
            ComponentDataFromEntity<Depth> depthLookup,
            WorkerAbility ability,
            float4 color)
        {
            if (current.x < size.x && current.y < size.y && current.y >= 0 && current.x >= 0)
            {
                Entity nextBlock = map[current];
                Depth nextDepth = depthLookup[nextBlock];
                nextDepth.Value += amount;
                buffer.SetComponent(nextBlock, nextDepth);
                buffer.AddComponent(nextBlock, new DrillHit() { WorkerAbility = ability, Power = amount, Color = color});
            }
        }
        
        protected override void OnUpdate()
        {
            if (groupSystem.IsReady == false)
            {
                return;
            }

            var map = groupSystem.BlocksMap;
            
            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            ComponentDataFromEntity<Depth> depthLookup = GetComponentDataFromEntity<Depth>();
            
            float time = (float)Time.ElapsedTime;

            float deltaTime = Time.DeltaTime;
            var size = groupSystem.GroupSize;

            NativeHashMap<int2, int> checkMap = new
                NativeHashMap<int2, int>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);
                
            NativeList<int2> pointBuffer = new
                NativeList<int2>(groupSystem.GroupSize.x * groupSystem.GroupSize.y,
                    Allocator.TempJob);
            
            var rand = Random.CreateFromIndex((uint)Time.ElapsedTime);;
            var isDrilledLookup = GetComponentDataFromEntity<IsBeingDrilled>();

            float3 origin = groupSystem.VisualOrigin;

            float scaleMultiplierThreshold = 0.25f;

            float yLerpSpeed = 75;
            
            float yLerpSpeedNoDrill = 10;
            
            Dependency = Entities.WithReadOnly(depthLookup)
                .ForEach((Entity entity,
                ref Worker worker,
                ref Translation translation, 
                ref NonUniformScale scale, 
                ref DestinationPoint destination,
                ref VerticalLimit verticalLimit,
                ref DrillPower power,
                in WorkerAnimations animations) =>
                {
                    float scaleMultiplier = math.max(scale.Value.x, scaleMultiplierThreshold);
                    
                    float drillTime = math.clamp(math.remap(worker.LastHitTime, worker.LastHitTime + power.Rate * scaleMultiplier, 0, 1, time), 0, 1);
                    
                    float3 visualDestination = origin + new float3(destination.Value.x, translation.Value.y, destination.Value.y);

                    bool inRange = math.distancesq(new float2(visualDestination.x, visualDestination.z),
                        new float2(translation.Value.x, translation.Value.z)) < 0.01f;
                    
                    if (inRange)
                    {
                        float y =
                            verticalLimit.Value + CurveUtil.Evaluate(ref animations.Bounce.Value.Keyframes, drillTime) * verticalLimit.FlightHeight;

                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, y, deltaTime * yLerpSpeed), translation.Value.z);
                    }
                    else
                    {
                        translation.Value = new float3(translation.Value.x, math.lerp(translation.Value.y, verticalLimit.Value + verticalLimit.FlightHeight, deltaTime * yLerpSpeedNoDrill), translation.Value.z);
                    }

                    if (time - worker.LastHitTime > power.Rate * scaleMultiplier && inRange)
                    {
                        worker.LastHitTime = time;
                        float newScale = scale.Value.x - worker.SizeLossPerHit;
                        scale.Value = new float3(newScale, newScale, newScale);
                        Depth depth = depthLookup[worker.CurrentBlock];
                        depth.Value += power.Amount * math.max(scale.Value.x, 0.25f);
                        buffer.AddComponent(worker.CurrentBlock, new DrillHit() { WorkerAbility = worker.Ability, Power = power.Amount });
                        int2 center = destination.Value;
                        verticalLimit.Value = -depth.Value;
                        buffer.SetComponent(worker.CurrentBlock, depth);
                        float4 color = worker.Color;
                        if (worker.Radius > 0)
                        {
                            float factor = 0.8f;
                            
                            IncreaseDepth(size, new int2(center.x + 1, center.y), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
    
                            IncreaseDepth(size, new int2(center.x - 1, center.y), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
               
                        }
                        
                        if (worker.Radius > 1)
                        {
                            float factor = 0.6f;
                            
                            IncreaseDepth(size, new int2(center.x + 1, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x - 1, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x + 1, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x - 1, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker.Ability, color);
                        }
                        
                        if (worker.Radius > 2)
                        {
                            float spread = 0.4f;
                            
                            IncreaseDepth(size, new int2(center.x + 2, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x - 2, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                        }
                        
                        if (worker.Radius > 3)
                        {
                            float spread = 0.2f;
                            
                            IncreaseDepth(size, new int2(center.x - 1, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x - 2, center.y + 1), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x - 2, center.y - 1), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x - 1, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x + 1, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x + 2, center.y - 1), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x + 2, center.y + 1), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x + 1, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                        }

                        
                        if (worker.Radius > 4)
                        {
                            float spread = 0.1f;
                            
                            IncreaseDepth(size, new int2(center.x - 3, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x + 3, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);

                            IncreaseDepth(size, new int2(center.x, center.y - 3), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                            
                            IncreaseDepth(size, new int2(center.x, center.y + 3), buffer, power.Amount * spread, map,
                                depthLookup, worker.Ability, color);
                        }
                        
                        verticalLimit.Value = -depth.Value;
                        buffer.SetComponent(worker.CurrentBlock, depth);
                        power.Hits++;
                        
                        if (power.Hits >= worker.MaxConsecutiveHits)
                        {
                            power.Hits = 0;
                            if (BlockUtil.GetClosestBlockOnSameLevel(rand, destination.Value, depthLookup, isDrilledLookup, map, checkMap, pointBuffer, size, out int2 next))
                            {
                                buffer.RemoveComponent<IsBeingDrilled>(worker.CurrentBlock); 
                                destination.Value = next;
                                worker.CurrentBlock = map[next];
                            }
                        }
                                
                        if (newScale <= 0.1f)
                        {
                            buffer.DestroyEntity(entity);
                            buffer.RemoveComponent<IsBeingDrilled>(worker.CurrentBlock); 
                        }
                    }
                
                }).Schedule(Dependency);

            Dependency.Complete();
            
            pointBuffer.Dispose(Dependency);
            checkMap.Dispose(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);

            CurrentJob = Dependency;
        }
    }
}