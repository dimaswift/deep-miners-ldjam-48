using System.Threading.Tasks;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class WorkerMoveSystem : SystemBase
    {
        private float size;

        private BlockGroupSystem groupSystem;

        protected override async void OnCreate()
        { 
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
            
            Dependency = Entities.WithReadOnly(dentMap).WithReadOnly(map).ForEach((Entity entity, 
                BlockGroupVisualOrigin origin, 
                MoveSpeed speed, 
                BlockPoint point, 
                ref VerticalVelocity velocity, 
                ref Translation translation) =>
            {
                float3 finalPos = origin.Value + new float3(point.Value.x, translation.Value.y, point.Value.z) * s;

                float y = translation.Value.y;
                var bottom = new int3(point.Value.x, point.Value.y + 1, point.Value.z);

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

                if (translation.Value.y > finalPos.y)
                {
                    velocity.Value += dt * 10;
                }
                else
                {
                    velocity.Value = 0;
                }

                finalPos.y = translation.Value.y + dt * velocity.Value;
                
                translation.Value = math.lerp(translation.Value, finalPos, dt * speed.Value);

            }).Schedule(Dependency);
            
        }
    }
}