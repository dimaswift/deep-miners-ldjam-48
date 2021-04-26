using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
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
        private BlobAssetReference<KeyframeBlobArray> moveCurve;

        private readonly Dictionary<WorkerConfig, RenderMeshDescription> cachedMeshDescriptions =
            new Dictionary<WorkerConfig, RenderMeshDescription>();

        public WorkerConfig DefaultWorker => config.workers[0];
        
        protected override async void OnCreate()
        {
            config = await Addressables.LoadAssetAsync<DrillConfig>("configs/drill").Task;
            await LoadConfigs(config.workers);

            foreach (WorkerConfig workerConfig in config.workers)
            {
                await Register(workerConfig);
            }
            
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            bounceCurve = CurveUtil.GetCurveBlobReference(config.drillBounceCurve.keys);
            moveCurve = CurveUtil.GetCurveBlobReference(config.drillMoveCurve.keys);
        }

        public async Task Register(WorkerConfig workerConfig)
        {
            RenderMeshDescription rend = await workerConfig.GetDescription();
            workerConfig.Color = rend.RenderMesh.material.color;
            cachedMeshDescriptions[workerConfig] = rend;
        }

        protected override void OnDestroy()
        {
            moveCurve.Dispose();
            bounceCurve.Dispose();
        }

        protected override void OnUpdate()
        {
            
        }

        public IEnumerable<WorkerConfig> GetWorkersConfigs() => config.workers;

        public Entity CreateWorker(WorkerConfig workerConfig, int2 position)
        {
            float3 worldPos = blockGroupSystem.VisualOrigin + new float3(position.x, 0, position.y);

            Entity block = blockGroupSystem.GetBlock(position);
             
            Entity entity = CreateBaseEntity(worldPos);

            Color color = workerConfig.Color;
            
            EntityManager.AddComponentData(entity, new Worker()
            {
                Ability = workerConfig.ability,
                CurrentBlock = block,
                Frequency = workerConfig.frequency,
                SizeLossPerHit = workerConfig.sizeLossPerHit,
                Radius = workerConfig.radius,
                MaxBounces = workerConfig.maxBounces,
                MarkDuration = workerConfig.markDuration,
                Timer = 0.75f,
                Color = new float4(color.r, color.g, color.b, color.a)
            } );

            EntityManager.AddComponentData(entity, new DestinationPoint() { Value = position, PreviousPoint = position});
            EntityManager.AddComponentData(entity, new VerticalLimit() { FlightHeight = Random.CreateFromIndex((uint)Time.ElapsedTime).NextInt(5, 8) });
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = workerConfig.size });
            EntityManager.AddComponentData(entity, new DrillPower() { Amount = workerConfig.power, Frequency = workerConfig.frequency });
            EntityManager.AddComponentData(entity, new WorkerAnimations() { Bounce = bounceCurve, Move = moveCurve });
            RenderMeshUtility.AddComponents(entity, EntityManager, cachedMeshDescriptions[workerConfig]);
            EntityManager.SetComponentData(entity, new Translation() { Value = blockGroupSystem.ToWorldPoint(position, 0) });
            return entity;
        }

    }
}