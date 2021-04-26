using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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

        private static void LeaveMark(int2 size, 
            int2 current,
            EntityCommandBuffer buffer, 
            float amount,
            NativeHashMap<int2, Entity> map,
            ComponentDataFromEntity<Depth> depthLookup,
            Worker worker)
        {
            if (current.x < size.x && current.y < size.y && current.y >= 0 && current.x >= 0)
            {
                Entity block = map[current];
                Depth depth = depthLookup[block];
                depth.Value += amount;
                buffer.SetComponent(block, depth);
                buffer.AddComponent(block,
                    new Mark()
                    {
                        Power = amount, 
                        Color = worker.Color,
                        Duration = worker.MarkDuration,
                        Block = block
                    });
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
            var markLookup = GetComponentDataFromEntity<Mark>();
            float3 origin = groupSystem.VisualOrigin;

            float scaleMultiplierThreshold = 0.25f;

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
                    float drillTime = worker.Timer;

                    bool isBouncing = power.Bounces < worker.MaxBounces;

                    if (isBouncing == false && worker.Timer >= 1)
                    {
                        power.Bounces = 0;
                    }
                    
                    float y =
                        verticalLimit.Value + CurveUtil.Evaluate(ref animations.Bounce.Value.Keyframes, drillTime) * verticalLimit.FlightHeight * scale.Value.x;
                    
                    if (isBouncing == false)
                    {
                        var t = CurveUtil.Evaluate(ref animations.Move.Value.Keyframes, drillTime);
                        float3 newPos = origin +
                                        math.lerp(
                                            new float3(destination.PreviousPoint.x, y, destination.PreviousPoint.y),
                                            new float3(destination.Value.x, y, destination.Value.y), t);
                        translation.Value = newPos;
                    }
                    else
                    {
                        translation.Value = new float3(translation.Value.x, y, translation.Value.z);
                    }

                    if (worker.Timer >= 1 && isBouncing)
                    {
                        worker.Timer = 0;
                        float newScale = scale.Value.x - worker.SizeLossPerHit;
                        scale.Value = new float3(newScale, newScale, newScale);
                        Depth depth = depthLookup[worker.CurrentBlock];
                        var centerMarkAmount = power.Amount * math.max(scale.Value.x, 0.1f);
                        int2 center = destination.Value;
                        verticalLimit.Value = -(depth.Value + centerMarkAmount);
                  
                        LeaveMark(size, center, buffer, centerMarkAmount, map,
                            depthLookup, worker);
                        
                        if (worker.Radius > 0)
                        {
                            float factor = 0.8f;
                            
                            LeaveMark(size, new int2(center.x + 1, center.y), buffer, power.Amount * factor, map,
                                depthLookup, worker);
    
                            LeaveMark(size, new int2(center.x - 1, center.y), buffer, power.Amount * factor, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup,  worker);
               
                        }
                        
                        if (worker.Radius > 1)
                        {
                            float factor = 0.6f;
                            
                            LeaveMark(size, new int2(center.x + 1, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup,  worker);

                            LeaveMark(size, new int2(center.x - 1, center.y - 1), buffer, power.Amount * factor, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x + 1, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x - 1, center.y + 1), buffer, power.Amount * factor, map,
                                depthLookup, worker);
                        }
                        
                        if (worker.Radius > 2)
                        {
                            float spread = 0.4f;
                            
                            LeaveMark(size, new int2(center.x + 2, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x - 2, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                        }
                        
                        if (worker.Radius > 3)
                        {
                            float spread = 0.2f;
                            
                            LeaveMark(size, new int2(center.x - 1, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x - 2, center.y + 1), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x - 2, center.y - 1), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x - 1, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x + 1, center.y - 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x + 2, center.y - 1), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x + 2, center.y + 1), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x + 1, center.y + 2), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                        }

                        
                        if (worker.Radius > 4)
                        {
                            float spread = 0.1f;
                            
                            LeaveMark(size, new int2(center.x - 3, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x + 3, center.y), buffer, power.Amount * spread, map,
                                depthLookup, worker);

                            LeaveMark(size, new int2(center.x, center.y - 3), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                            
                            LeaveMark(size, new int2(center.x, center.y + 3), buffer, power.Amount * spread, map,
                                depthLookup, worker);
                        }
     
                        power.Bounces++;
                        
                        if (power.Bounces >= worker.MaxBounces)
                        {
                            if (BlockUtil.GetClosestBlockOnSameLevel(rand, destination.Value, depthLookup, isDrilledLookup, map, checkMap, pointBuffer, size, out int2 next))
                            {
                                buffer.RemoveComponent<IsBeingDrilled>(worker.CurrentBlock);
                                destination.PreviousPoint = destination.Value;
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
                    
                    float scaleMultiplier = math.max(scale.Value.x, scaleMultiplierThreshold);
                    worker.Timer += deltaTime / worker.Frequency / scaleMultiplier;
                    worker.Timer = math.min(1, worker.Timer);

                }).Schedule(Dependency);

            Dependency.Complete();
            
            pointBuffer.Dispose(Dependency);
            checkMap.Dispose(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);

            CurrentJob = Dependency;
        }
    }
}