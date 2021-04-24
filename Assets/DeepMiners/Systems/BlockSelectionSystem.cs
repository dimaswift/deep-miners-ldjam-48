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

        private Entity debugEntity;
 
        private bool isReady;
        

        protected override async void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();

            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
            
            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            
            GameObject selectionRendererPrefab = await Addressables.LoadAssetAsync<GameObject>("prefabs/selection").Task;
            selectionRenderer = selectionRendererPrefab.GetComponent<RenderMeshPrefab>().GetDescription();

            debugEntity = CreateDebugEntity();


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

            var map = blockGroupSystem.BlocksMap;

            int3? hoveringBlock = blockGroupSystem.ScreenToBlockPoint(1);

            EntityCommandBuffer buffer = commandBufferSystem.CreateCommandBuffer();
            
            int3? current = blockGroupSystem.ScreenToBlockPoint(1);

            int2 groupSize = blockGroupSystem.GroupSize;

            int currentDepth = blockGroupSystem.CurrentDepth;

            if (current.HasValue)
            {
                int3 c = current.Value;
                
                NativeArray<int3> closestResult = new NativeArray<int3>(1, Allocator.TempJob) {[0] = new int3(-1, -1, -1)};

                NativeHashMap<int3, int> checkMap = new
                    NativeHashMap<int3, int>(blockGroupSystem.GroupSize.x * blockGroupSystem.GroupSize.y,
                        Allocator.TempJob);
                
                NativeList<int3> checkList = new
                    NativeList<int3>(blockGroupSystem.GroupSize.x * blockGroupSystem.GroupSize.y,
                        Allocator.TempJob);
                
                
                
                Dependency = Job.WithReadOnly(map).WithCode(() => 
                {
                    if (BlockUtil.GetClosestBlockOnSameLevel(c, map, checkMap, checkList, groupSize, out int3 closest))
                    {
                        closestResult[0] = closest;
                    }

                }).Schedule(Dependency);

                float3 origin = blockGroupSystem.VisualOrigin;
                float blockSize = blockGroupSystem.BlockSize;

                Entity e = debugEntity;
                
                Dependency = Job.WithReadOnly(closestResult).WithCode(() =>
                {
                    if (closestResult[0].x >= 0)
                    {
                        buffer.SetComponent(e, new Translation() { Value = BlockUtil.ToWorld(closestResult[0], origin, blockSize)});
                    }

                }).Schedule(Dependency);

                closestResult.Dispose(Dependency);

                checkMap.Dispose(Dependency);

                checkList.Dispose(Dependency);
                
                commandBufferSystem.AddJobHandleForProducer(Dependency);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveringBlock.HasValue)
                {
                    int3 b = hoveringBlock.Value;

                    Dependency = blockGroupSystem.FindBlock(hoveringBlock.Value, out var res, Dependency);

                    EntityCommandBuffer newBuff = commandBufferSystem.CreateCommandBuffer();
                    
                    Dependency = Job.WithCode(() =>
                    {
                        if (res[0] != Entity.Null)
                        {
                            newBuff.AddComponent(res[0], new Dent() {Value = 0.75f });
                        }

                    }).Schedule(Dependency);

                   // Dependency = blockGroupSystem.DestroyBlock(b, Dependency);
                    
                    commandBufferSystem.AddJobHandleForProducer(Dependency);

                    res.Dispose(Dependency);

                  
                }
            }

        }

    }
}