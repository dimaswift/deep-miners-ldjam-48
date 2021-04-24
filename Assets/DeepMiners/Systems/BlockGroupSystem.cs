using System.Threading;
using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Jobs;
using DeepMiners.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    public class BlockGroupSystem : ConfigurableSystem<BlockConfig>
    {
        public float BlockSize => config.blockSize;

        public int CurrentDepth => currentDepth;
        public int2 GroupSize => config.size;
        
        public bool IsReady { get; private set; }
        public NativeHashMap<int3, Entity> BlocksMap => blocksMap;
        public float3 VisualOrigin => visualOrigin;
        
        private BlockGroupConfig config;
        private Camera cam;
        private Random random = Random.CreateFromIndex(0);
        private float3 visualOrigin;
        private NativeHashMap<int3, Entity> blocksMap;
        private int2 size;

        public JobHandle? ModificationJob;
        
        private int currentDepth;
        
        protected override async void OnCreate()
        {
            config = await Addressables.LoadAssetAsync<BlockGroupConfig>("configs/defaultBlockGroup").Task;
            size = config.size;
            blocksMap = new NativeHashMap<int3, Entity>(config.maxDepth * config.size.x * config.size.x,
                Allocator.Persistent);
            await LoadConfigs(config.blocks);
            AddGroup(config.initialDepth);

            while (cam == null)
            {
                await Task.Yield();
            }
            
            IsReady = true;
        }

        public bool ContainsPoint(int3 point) => BlockUtil.ContainsPoint(point, size, currentDepth);

        public float3 ToWorldPoint(int3 blockPoint) =>
            visualOrigin + new float3(blockPoint.x, -blockPoint.y, blockPoint.z) * config.blockSize;
        
        public Entity GetBlock(int3 point) => blocksMap[point];

        protected override void OnDestroy()
        {
            blocksMap.Dispose();
        }

        protected override async Task LoadConfigs(BlockConfig[] configs)
        {
            visualOrigin = GameObject.Find("Origin").transform.position;
            await base.LoadConfigs(configs);
        }

        public int3? ScreenToBlockPoint(int level)
        {
            var plane = new Plane(Vector3.up, (Vector3)visualOrigin + new Vector3(0, -level + 0.5f, 0) * config.blockSize);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enterDistance))
            {
                float3 worldPoint = ray.GetPoint(enterDistance);
                float3 result = worldPoint - visualOrigin;

                result /= config.blockSize;
                var point = new int3(Mathf.RoundToInt(result.x), level, Mathf.RoundToInt(result.z));

                if (ContainsPoint(point) == false)
                {
                    return null;
                }

                return point;
            }

            return null;
        }

        public Entity CreateBlock(BlockType type, int3 position)
        {
            float blockSize = config.blockSize;
            Entity entity = CreateBaseEntity(visualOrigin + new float3(position.x, -position.y, position.z) * blockSize);
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = new float3(blockSize, blockSize, blockSize) });
            EntityManager.AddComponentData(entity, new Block() { Type = type });
            EntityManager.AddComponentData(entity, new BlockPoint() { Value = position });
            EntityManager.AddComponentData(entity, new BlockGroupVisualOrigin() { Value = visualOrigin });
            EntityManager.AddComponentData(entity, new VerticalVelocity() { Value = 0 });
            EntityManager.AddComponentData(entity, new Dent() { Value = 1 });
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            
            return entity;
        }
        
        public void DestroyBlock(int3 position)
        {
            Entity block = BlocksMap[position];
            EntityManager.DestroyEntity(block);
            blocksMap[position] = Entity.Null;
        }

        public bool HasBlock(int3 position)
        {
            return BlockUtil.HasBlock(position, size, currentDepth, blocksMap);
        }

        public JobHandle FindBlock(int3 point, out NativeArray<Entity> result, JobHandle deps)
        {
            result = new NativeArray<Entity>(1, Allocator.TempJob);
            return new FindBlockJob()
            {
                Point = point,
                Map = blocksMap,
                Result = result
            }.Schedule(deps);
        }

        public JobHandle WaitForModificationJob(JobHandle deps)
        {
            if (ModificationJob.HasValue && ModificationJob.Value.IsCompleted == false)
            {
                return JobHandle.CombineDependencies(ModificationJob.Value, deps);
            }

            return deps;
        }
        
        public JobHandle DestroyBlock(int3 point, JobHandle deps)
        {
            ModificationJob = new DestroyBlockJob()
            {
                Point = point,
                Map = blocksMap,
            }.Schedule(deps);

            return ModificationJob.Value;
        }
        
        private void AddGroup(int depth)
        {
            for (int level = currentDepth; level < currentDepth + depth; level++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int z = 0; z < size.y; z++) 
                    {
                        var point = new int3(x, level, z);
                        if (level > 0)
                        {
                            BlockType type = (BlockType) random.NextInt(0, (int) BlockType.BlocksAmount);
                            blocksMap.Add(point, CreateBlock(type, point));
                        }
                        else
                        {
                            blocksMap.Add(point, Entity.Null);
                        }
                     
                    }
                }
            }
            
            currentDepth += depth;
        }
        
        protected override void OnUpdate()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (ModificationJob.HasValue && ModificationJob.Value.IsCompleted)
            {
                ModificationJob = null;
            }
           
        }
    }
}