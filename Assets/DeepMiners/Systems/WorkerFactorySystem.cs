using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
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
        }

        protected override void OnDestroy()
        {
            bounceCurve.Dispose();
        }

        protected override void OnUpdate()
        {
            
        }
        public WorkerConfig GetConfig(WorkerType type) => Configs[(int) type];
        
        public Entity CreateWorker(WorkerType type, int2 position)
        {
            WorkerConfig workerConfig = Configs[(int)type];
            
            float3 worldPos = blockGroupSystem.VisualOrigin + new float3(position.x, 0, position.y);

            Entity block = blockGroupSystem.GetBlock(position);
             
            Entity entity = CreateBaseEntity(worldPos);
            EntityManager.AddComponentData(entity, new Worker()
            {
                Type = type,
                CurrentBlock = block,
                LastHitTime = Time.DeltaTime,
                SizeLossPerHit = workerConfig.sizeLossPerHit,
                Radius = workerConfig.radius,
                MaxConsecutiveHits = workerConfig.maxConsecutiveHits
            } );
            EntityManager.AddComponentData(entity, new MoveSpeed() { Value = workerConfig.moveSpeed });
            EntityManager.AddComponentData(entity, new DestinationPoint() { Value = position });
            EntityManager.AddComponentData(entity, new VerticalLimit() { FlightHeight = Random.CreateFromIndex((uint)Time.ElapsedTime).NextInt(5, 8) });
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = workerConfig.size });
            EntityManager.AddComponentData(entity, new DrillPower() { Amount = workerConfig.power, Rate = workerConfig.drillRate });
            EntityManager.AddComponentData(entity, new WorkerAnimations() { Bounce = bounceCurve });
            
            EntityManager.AddComponentData(entity, new IgnoreGravity());
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            EntityManager.SetComponentData(entity, new Translation() { Value = blockGroupSystem.ToWorldPoint(position, 0) });
            return entity;
        }

    }
}