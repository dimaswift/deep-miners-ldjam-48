using System.Threading.Tasks;
using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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

            float deltaTime = Time.DeltaTime;
            float blockSize = size;
            float3 origin = groupSystem.VisualOrigin;

            Dependency = Entities.ForEach((Entity entity,
                MoveSpeed speed,
                DestinationPoint destination,
                ref Translation translation) =>
            {

                int2 point = destination.Value;
                
                float3 finalPos = origin + new float3(point.x, translation.Value.y, point.y) * blockSize;

                float actualSpeed = speed.Value;
                
                float3 interpolated = math.lerp(translation.Value, finalPos, deltaTime * actualSpeed);

                interpolated.y = translation.Value.y;

                translation.Value = interpolated;

            }).Schedule(Dependency);
            
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}