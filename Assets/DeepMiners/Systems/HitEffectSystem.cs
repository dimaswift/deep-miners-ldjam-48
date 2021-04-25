using DeepMiners.Config;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class HitEffectSystem : SystemBase
    {
        private DrillConfig drillConfig;
        private RenderMeshDescription effectMesh;
        private WorkerFactorySystem workerFactorySystem;
        private EntityCommandBufferSystem commandBufferSystem;
        private quaternion rotation;
        private EntityQuery query;
        private AudioSource hitSource;

        private float lastSoundPlayTime;
        
        protected override async void OnCreate()
        {
            
            workerFactorySystem = World.GetExistingSystem<WorkerFactorySystem>();
            commandBufferSystem = World.GetExistingSystem<EntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(DrillHit), typeof(Translation), typeof(Depth));
            drillConfig = await Addressables.LoadAssetAsync<DrillConfig>("configs/drill").Task;
            effectMesh = drillConfig.hitEffect.GetDescription();
            rotation = drillConfig.hitEffect.transform.rotation;
        }

        protected override void OnUpdate()
        {
            if (hitSource == null)
            {
                hitSource = GameObject.Find("HitAudioSource").GetComponent<AudioSource>();
            }
            
            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();

            quaternion rot = rotation;

            if (query.CalculateEntityCount() == 0)
            {
                return;
            }

            NativeArray<Entity> result = query.ToEntityArray(Allocator.Temp);
            NativeArray<Translation> positions = query.ToComponentDataArray<Translation>(Allocator.Temp);
            NativeArray<Depth> dents = query.ToComponentDataArray<Depth>(Allocator.Temp);
            NativeArray<DrillHit> hits = query.ToComponentDataArray<DrillHit>(Allocator.Temp);
            
            for (int i = 0; i < result.Length; i++)
            {
                Entity entity = result[i];
                Color color = workerFactorySystem.GetConfig(hits[i].WorkerType).color;
                Entity effect = buffer.CreateEntity();
                float3 pos = positions[i].Value;
                pos.y = -dents[i].Value + 0.01f;
                buffer.AddComponent(effect, new Translation() { Value = pos });
                buffer.AddComponent(effect, new LocalToWorld());
                buffer.AddComponent(effect, new Rotation() { Value = rot });
                buffer.AddComponent(effect, new Effect() { Timer = 0, Duration = 0.25f, Size = 1f});
                buffer.AddComponent(effect, new Scale() { Value = 1 });
                buffer.AddComponent(effect, new BlockColor() { Value = new float4(color.r, color.g, color.b, color.a) }); 
                
                buffer.RemoveComponent<DrillHit>(entity);
                RenderMeshUtility.AddComponents(effect, buffer, effectMesh);

                if (Time.ElapsedTime - lastSoundPlayTime > 0.05f)
                {
                    lastSoundPlayTime = (float)Time.ElapsedTime;
                    AudioClip sound = drillConfig.hitSounds[math.clamp((int)(dents[i].Value * drillConfig.hitSounds.Length), 0, drillConfig.hitSounds.Length - 1)];
                    hitSource.PlayOneShot(sound);
                }
            }

            positions.Dispose();
            dents.Dispose();
            result.Dispose();
            hits.Dispose();
        }
    }
}