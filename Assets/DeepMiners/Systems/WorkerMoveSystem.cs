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

            Dependency = Entities.ForEach((Entity entity, 
                BlockGroupVisualOrigin origin, 
                MoveSpeed speed,
                DestinationPoint destination,
                ref Translation translation) =>
            {

                int3 point = destination.Value;
                
                float3 finalPos = origin.Value + new float3(point.x, translation.Value.y, point.z) * s;

                float actualSpeed = speed.Value;
                
                float3 interpolated = math.lerp(translation.Value, finalPos, dt * actualSpeed);

                interpolated.y = translation.Value.y;

                translation.Value = interpolated;

            }).Schedule(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);
            
        }
    }
}