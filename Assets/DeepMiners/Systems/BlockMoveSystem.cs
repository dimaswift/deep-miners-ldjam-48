using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class BlockMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            Entities.ForEach((Entity entity, BlockGroupVisualOrigin origin, MoveSpeed speed, BlockPoint point, ref Translation translation) =>
            {
                float3 targetPos = origin.Value + point.Value;
                float dist = math.distancesq(targetPos, translation.Value);
                if (dist > 0)
                {
                    translation.Value += math.normalize(targetPos - translation.Value) * speed.Value * dt;
                }

            }).Schedule();
            
        }
    }
}