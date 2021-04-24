using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class GravitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            const float gravity = 9f;

            float dt = Time.DeltaTime;
            
            Entities.WithNone<IgnoreGravity>().ForEach((ref Translation translation, ref VerticalVelocity velocity, in VerticalLimit limit) =>
            {
                float3 newPos = translation.Value;
                if (translation.Value.y > limit.Value)
                {
                    velocity.Value -= dt * gravity;
                }
                else
                {
                    velocity.Value = -velocity.Value * limit.Bounciness;
                }

                if (translation.Value.y < limit.Value)
                {
                    newPos.y = limit.Value;
                }

                if (math.abs(velocity.Value) > 0)
                {
                    newPos.y += dt * velocity.Value;
                } 
                
                translation.Value = newPos;

            }).Schedule();
        }
    }
}