using System.Threading.Tasks;
using DeepMiners.Data;
using DeepMiners.Prefabs;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateBefore(typeof(WorkerMoveSystem))]
    public class BlockSelectionSystem : SystemBase
    {
        private BlockGroupSystem blockGroupSystem;

        private RenderMeshDescription selectionRenderer;
        private WorkerFactorySystem workerFactorySystem;

        private bool isReady;
        

        protected override async void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            workerFactorySystem = World.GetExistingSystem<WorkerFactorySystem>();
           
            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            
            GameObject selectionRendererPrefab = await Addressables.LoadAssetAsync<GameObject>("prefabs/selection").Task;
            selectionRenderer = selectionRendererPrefab.GetComponent<RenderMeshPrefab>().GetDescription();
            
            isReady = true;

        }
        
        private Entity CreateDebugEntity()
        {
            Entity entity = EntityManager.CreateEntity(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld),
                typeof(NonUniformScale));
            
            RenderMeshUtility.AddComponents(entity, EntityManager, selectionRenderer);
            EntityManager.AddComponentData(entity, new BlockColor() {Value = new float4(0,0,1,0.5f)});
            float blockSize = blockGroupSystem.BlockSize;
            EntityManager.SetComponentData(entity, new NonUniformScale()  { Value = new float3(blockSize,blockSize,blockSize) * 1.01f }  );
            return entity;
        }
        
        public void Select(int2 point)
        {
            Entity selectionMesh = EntityManager.CreateEntity(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld),
                typeof(NonUniformScale));
            
            RenderMeshUtility.AddComponents(selectionMesh, EntityManager, selectionRenderer);
            float blockSize = blockGroupSystem.BlockSize;
            EntityManager.SetComponentData(selectionMesh, new NonUniformScale()  { Value = new float3(blockSize,blockSize,blockSize) }  );
            EntityManager.SetComponentData(selectionMesh, new Translation()  { Value = blockGroupSystem.ToWorldPoint(point, 0)}  );
        }
        
        
        
        protected override void OnUpdate()
        {
            if (!isReady)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                int2? current = blockGroupSystem.ScreenToBlockPoint(1);

                if (current.HasValue)
                {
                    int2 c = current.Value;
                    workerFactorySystem.CreateWorker(WorkerType.ShovelDigger, c);
                }
            }
        

        }

    }
}