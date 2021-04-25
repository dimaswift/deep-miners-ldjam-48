using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class WorkerFactorySystem : ConfigurableSystem<WorkerConfig>
    {
        private BlockGroupSystem blockGroupSystem;
        private DrillConfig config;
        private BlobAssetReference<KeyframeBlobArray> bounceCurve;
 
        protected override async void OnCreate()
        {
            config = await Addressables.LoadAssetAsync<DrillConfig>("configs/drill").Task;
            await LoadConfigs(config.workers);
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();

            bounceCurve = CurveUtil.GetCurveBlobReference(config.drillBounceCurve.keys);
            
            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }

            // CreateWorker(WorkerType.ShovelDigger, new int3(5, 0, 5));
            //
            // await Task.Delay(100);
            //
            // CreateWorker(WorkerType.ShovelDigger, new int3(4, 0, 5));
            //
            // await Task.Delay(100);
            //
            // CreateWorker(WorkerType.ShovelDigger, new int3(0, 0, 0));
            //
            // await Task.Delay(100);
            //
            // CreateWorker(WorkerType.ShovelDigger, new int3(9, 0, 9));
            //
            // await Task.Delay(100);
            //
            // CreateWorker(WorkerType.ShovelDigger, new int3(9, 0, 2));
            //
            // await Task.Delay(100);
            //
            // CreateWorker(WorkerType.ShovelDigger, new int3(9, 0, 5));
            //
            // await Task.Delay(100);
            //
            // await Task.Delay(1000);
            
         //  EntityManager.SetComponentData(worker, new BlockPoint() {Value = new int3(9,0,9)}); 
        }

        protected override void OnDestroy()
        {
            bounceCurve.Dispose();
        }

        protected override void OnUpdate()
        {
            
        }

        public WorkerConfig GetConfig(WorkerType type) => Configs[(int) type];
        
        public Entity CreateWorker(WorkerType type, int3 position)
        {
            WorkerConfig workerConfig = Configs[(int)type];
            
            float3 worldPos = blockGroupSystem.VisualOrigin + position;

            Entity block = blockGroupSystem.GetBlock(new int3() {x = position.x, y = 1, z = position.z});
             
            Entity entity = CreateBaseEntity(worldPos);
            EntityManager.AddComponentData(entity, new Worker()
            {
                Type = type,
                CurrentBlock = block,
                LastDentTime = Time.DeltaTime,
                Damping = 0.5f,
                SizeLossPerHit = workerConfig.sizeLossPerHit
            } );
            EntityManager.AddComponentData(entity, new MoveSpeed() { Value = workerConfig.moveSpeed });
            EntityManager.AddComponentData(entity, new DestinationPoint() { Value = position });
            EntityManager.AddComponentData(entity, new BlockGroupVisualOrigin() { Value = blockGroupSystem.VisualOrigin });
            EntityManager.AddComponentData(entity, new VerticalVelocity() { Value = 0 });
            EntityManager.AddComponentData(entity, new VerticalLimit() { Value = -10, FlightHeight = Random.CreateFromIndex((uint)Time.ElapsedTime).NextInt(5, 8) });
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = workerConfig.size });
            EntityManager.AddComponentData(entity, new DrillPower() { Amount = workerConfig.power, Rate = workerConfig.drillRate });
            EntityManager.AddComponentData(entity, new DrillAnimations() { Bounce = bounceCurve });

            
            EntityManager.AddComponentData(entity, new IgnoreGravity());
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            EntityManager.SetComponentData(entity, new Translation() { Value = blockGroupSystem.ToWorldPoint(position) });
            return entity;
        }

    }
}