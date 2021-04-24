using System.Threading.Tasks;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class WorkerMoveSystem : SystemBase
    {
        private float size;

        private BlockGroupSystem groupSystem;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override async void OnCreate()
        {
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
            groupSystem = World.GetOrCreateSystem<BlockGroupSystem>();

            while (groupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            
            size = groupSystem.BlockSize;
        }
        
        protected override void OnUpdate()
        {
            if (groupSystem.IsReady == false)
            {
                return;
            }

            Dependency = groupSystem.WaitForModificationJob(Dependency);

            float dt = Time.DeltaTime;
            float s = size;
            int depth = groupSystem.CurrentDepth;
            NativeHashMap<int3, Entity> map = groupSystem.BlocksMap;
            ComponentDataFromEntity<Dent> dentMap = GetComponentDataFromEntity<Dent>();

            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();

            Dependency = Entities.WithReadOnly(dentMap).WithReadOnly(map).ForEach((Entity entity, 
                BlockGroupVisualOrigin origin, 
                MoveSpeed speed,
                DestinationPoint destination,
                ref Translation translation) =>
            {

                int3 point = destination.Value;
                float3 finalPos = origin.Value + new float3(point.x, translation.Value.y, point.z) * s;

                float y = translation.Value.y;
                var bottom = new int3(point.x, point.y + 1, point.z);

                while (bottom.y < depth)
                { 
                    if (map.ContainsKey(bottom))
                    {
                        Entity block = map[bottom];
                        if (block != Entity.Null)
                        {
                            if (dentMap.HasComponent(block))
                            {
                                Dent dent = dentMap[block];
                                y = (-bottom.y) - ((1f - dent.Value) * s);
                            }
                            else
                            {
                                y = -bottom.y * s;
                            }
                            break;
                        }
                    }
                    bottom.y++;
                }
            
                finalPos.y = y + 1;

                buffer.SetComponent(entity, new VerticalLimit() { Value = finalPos.y } );

                float actualSpeed = speed.Value;
                
                float xzDist = math.distance(new float2(translation.Value.x, translation.Value.z),
                    new float2(finalPos.x, finalPos.z));

                if (xzDist > 0 && xzDist < s)
                {
                    actualSpeed *= xzDist;
                }

                float3 interpolated = Vector3.MoveTowards(translation.Value, finalPos, dt * actualSpeed);

                interpolated.y = translation.Value.y;

                translation.Value = interpolated;
            

            }).Schedule(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);
            
        }
    }
}