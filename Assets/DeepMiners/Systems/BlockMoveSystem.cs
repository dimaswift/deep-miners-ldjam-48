using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class BlockMoveSystem : SystemBase
    {
        private float size;

        protected override void OnStartRunning()
        {
            var groupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            size = groupSystem.BlockSize;
        }

        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            float s = size;
            Entities.ForEach((Entity entity, BlockGroupVisualOrigin origin, MoveSpeed speed, BlockPoint point, ref Translation translation) =>
            {
                float3 targetPos = origin.Value + new float3(point.Value.x, translation.Value.y, point.Value.z) * s;
                float dist = math.distancesq(targetPos, translation.Value);
                if (dist > 0)
                {
                    translation.Value += math.normalize(targetPos - translation.Value) * speed.Value * dt;
                }

            }).Schedule();
            
        }
    }
}