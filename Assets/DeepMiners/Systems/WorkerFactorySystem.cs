using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.AddressableAssets;

namespace Systems
{
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class WorkerFactorySystem : ConfigurableSystem<WorkerConfig>
    {
        private BlockGroupSystem blockGroupSystem;
        private DrillConfig config;
        
        protected override async void OnCreate()
        {
            config = await Addressables.LoadAssetAsync<DrillConfig>("configs/workerFactory").Task;
            await LoadConfigs(config.workers);
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            await Task.Yield();
            Entity worker = CreateWorker(WorkerType.ShovelDigger, new int3(5, 0, 5));
            await Task.Delay(1000);
            
         //  EntityManager.SetComponentData(worker, new BlockPoint() {Value = new int3(9,0,9)}); 
        }

        protected override void OnUpdate()
        {
            
        }

        public Entity CreateWorker(WorkerType type, int3 position)
        {
            Entity entity = CreateBaseEntity(blockGroupSystem.VisualOrigin + position);
            EntityManager.AddComponentData(entity, new Worker() { Type = type } );
            EntityManager.AddComponentData(entity, new MoveSpeed() { Value = 1f });
            EntityManager.AddComponentData(entity, new BlockPoint() { Value = position });
            EntityManager.AddComponentData(entity, new BlockGroupVisualOrigin() { Value = blockGroupSystem.VisualOrigin });
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            EntityManager.SetComponentData(entity, new Translation() { Value = blockGroupSystem.ToWorldPoint(position) });
            return entity;
        }

    }
}