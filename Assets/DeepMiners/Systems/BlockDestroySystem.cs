using DeepMiners.Data;
using Unity.Entities;
using Unity.Jobs;

namespace Systems
{
    [UpdateAfter(typeof(LateSimulationSystemGroup))]
    public class BlockDestroySystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private BlockGroupSystem blockGroupSystem;
    
        protected override void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }
    
        protected override void OnUpdate()
        {
            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            var map = blockGroupSystem.BlocksMap;

            Entities.ForEach((Entity entity, in DestroyBlock block, in BlockPoint point) =>
            {
                buffer.DestroyEntity(entity);
                map[point.Value] = Entity.Null;

            }).Run();
        }
    }
}