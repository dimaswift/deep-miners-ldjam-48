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
        private EntityCommandBufferSystem commandBufferSystem;

        private RenderMeshDescription selectionRenderer;
        private WorkerFactorySystem workerFactorySystem;
        private NativeArray<int3> selection;
        
        private Entity debugEntity;
 
        private bool isReady;
        

        protected override async void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            workerFactorySystem = World.GetExistingSystem<WorkerFactorySystem>();
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            selection = new NativeArray<int3>(1, Allocator.Persistent) {[0] = new int3(-1, -1, -1)};

            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            
            GameObject selectionRendererPrefab = await Addressables.LoadAssetAsync<GameObject>("prefabs/selection").Task;
            selectionRenderer = selectionRendererPrefab.GetComponent<RenderMeshPrefab>().GetDescription();
            
            isReady = true;

        }

        protected override void OnDestroy()
        {
            selection.Dispose();
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
        
        public void Select(int3 point)
        {
            Entity selectionMesh = EntityManager.CreateEntity(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld),
                typeof(NonUniformScale));
            
            RenderMeshUtility.AddComponents(selectionMesh, EntityManager, selectionRenderer);
            float blockSize = blockGroupSystem.BlockSize;
            EntityManager.SetComponentData(selectionMesh, new NonUniformScale()  { Value = new float3(blockSize,blockSize,blockSize) }  );
            EntityManager.SetComponentData(selectionMesh, new Translation()  { Value = blockGroupSystem.ToWorldPoint(point)}  );
        }
        
        
        
        protected override void OnUpdate()
        {
            if (!isReady)
            {
                return;
            }

            Dependency = blockGroupSystem.WaitForModificationJob(Dependency);
            
            if (selection[0].x >= 0)
            {
                workerFactorySystem.CreateWorker(WorkerType.ShovelDigger, selection[0]);

                selection[0] = new int3(-1, -1, -1);
            }
            
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            var map = blockGroupSystem.BlocksMap;
            
            int3? current = blockGroupSystem.ScreenToBlockPoint(1);

            var depth = blockGroupSystem.CurrentDepth;
            
            int2 groupSize = blockGroupSystem.GroupSize;

            Random random = Random.CreateFromIndex((uint)Time.ElapsedTime);

            if (current.HasValue)
            {
                int3 c = current.Value;

                NativeHashMap<int3, int> checkMap = new
                    NativeHashMap<int3, int>(blockGroupSystem.GroupSize.x * blockGroupSystem.GroupSize.y,
                        Allocator.TempJob);
                
                NativeList<int3> checkList = new
                    NativeList<int3>(blockGroupSystem.GroupSize.x * blockGroupSystem.GroupSize.y,
                        Allocator.TempJob);
                
                var isDrilled = GetComponentDataFromEntity<IsBeingDrilling>();

                var s = selection;
                
                Dependency = Job.WithReadOnly(map).WithReadOnly(isDrilled).WithCode(() =>
                {
                    c.y = BlockUtil.GetHighestNonEmptyBlockLevel(map, groupSize, depth, 1);
                    if (BlockUtil.GetClosestBlockOnSameLevel(random, c, isDrilled, map, checkMap, checkList, groupSize, out int3 closest))
                    {
                        s[0] = closest;
                    }

                }).Schedule(Dependency);
                
                checkMap.Dispose(Dependency);

                checkList.Dispose(Dependency);
                
                commandBufferSystem.AddJobHandleForProducer(Dependency);
            }
            
           

        }

    }
}