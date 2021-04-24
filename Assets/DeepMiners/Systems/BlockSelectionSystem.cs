using DeepMiners.Data;
using DeepMiners.Prefabs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class BlockSelectionSystem : SystemBase
    {
        private BlockGroupSystem blockGroupSystem;

        private int3? prevHoveringBlock;

        private NativeHashMap<int3, Entity> selection;

        private RenderMeshDescription selectionRenderer;

        protected override void OnDestroy()
        {
            selection.Dispose();
        }

        protected async override void OnCreate()
        {
            base.OnCreate();
            GameObject selectionRendererPrefab = await Addressables.LoadAssetAsync<GameObject>("prefabs/selection").Task;
            selectionRenderer = selectionRendererPrefab.GetComponent<RenderMeshPrefab>().GetDescription();
            
            selection = new NativeHashMap<int3, Entity>(128, Allocator.Persistent);
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
        }

        public void Select(int3 point)
        {
            if (selection.ContainsKey(point))
            {
                return;
            }

            Entity selectionMesh = EntityManager.CreateEntity(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld),
                typeof(NonUniformScale));
            
            RenderMeshUtility.AddComponents(selectionMesh, EntityManager, selectionRenderer);
            float blockSize = blockGroupSystem.BlockSize;
            EntityManager.SetComponentData(selectionMesh, new NonUniformScale()  { Value = new float3(blockSize,blockSize,blockSize) }  );
            EntityManager.SetComponentData(selectionMesh, new Translation()  { Value = blockGroupSystem.ToWorldPoint(point)}  );
            selection.Add(point, selectionMesh);
        }

        public void Deselect(int3 point)
        {
            if (selection.ContainsKey(point) == false)
            {
                return;
            }

            Entity selectionMesh = selection[point];
            
            EntityManager.DestroyEntity(selectionMesh);

            selection.Remove(point);
        }

        
        protected override void OnUpdate()
        {
            if (blockGroupSystem == null)
            {
                return;
            }

            int3? hoveringBlock = blockGroupSystem.ScreenToBlockPoint(1);

            if (hoveringBlock.HasValue)
            {
                if (prevHoveringBlock.HasValue == false 
                    || (prevHoveringBlock.HasValue && !prevHoveringBlock.Equals(hoveringBlock.Value)))
                {

                    if (blockGroupSystem.HasBlock(hoveringBlock.Value) == false)
                    {
                        if (prevHoveringBlock.HasValue)
                        {
                            Deselect(prevHoveringBlock.Value);
                            prevHoveringBlock = null;
                        }
                        return;
                    }
                    
                    Entity selectedBlock = blockGroupSystem.GetBlock(hoveringBlock.Value);

                    if (prevHoveringBlock.HasValue)
                    {
                        EntityManager.RemoveComponent<BlockSelection>(selectedBlock);
                    }
                    
                    if (EntityManager.HasComponent<BlockSelection>(selectedBlock) == false)
                    {
                        EntityManager.AddComponentData(selectedBlock, new BlockSelection());
                    }
                    
                    EntityManager.AddComponentData(selectedBlock, new BlockSelection());

                    if (prevHoveringBlock.HasValue)
                    {
                        Deselect(prevHoveringBlock.Value);
                    }
                    
                    prevHoveringBlock = hoveringBlock.Value;
                    
                    Select(hoveringBlock.Value);
                }
            }
            else
            {
                if (prevHoveringBlock.HasValue)
                {
                    Deselect(prevHoveringBlock.Value);
                    prevHoveringBlock = null;
                }
            }
            
            
            if (Input.GetMouseButtonDown(0))
            {
                if (prevHoveringBlock.HasValue && blockGroupSystem.HasBlock(prevHoveringBlock.Value))
                {
                    float dent = UnityEngine.Random.value;
                    EntityManager.AddComponentData(blockGroupSystem.GetBlock(prevHoveringBlock.Value), new Dent() {Value = dent });
                }
            }

        }
    }
}