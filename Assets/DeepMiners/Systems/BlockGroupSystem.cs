using System.Collections.Generic;
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
        
        public int2 GroupSize => config.size;
        
        public bool IsReady { get; private set; }
        public NativeHashMap<int2, Entity> BlocksMap => blocksMap;
        public float3 VisualOrigin => visualOrigin;
        
        private BlockGroupConfig config;
        private Camera cam;
        private Random random = Random.CreateFromIndex(0);
        private float3 visualOrigin;
        private NativeHashMap<int2, Entity> blocksMap;
        private int2 size;

        private readonly List<BlockType> typePool = new List<BlockType>();

        protected override async void OnCreate()
        {
            config = await Addressables.LoadAssetAsync<BlockGroupConfig>("configs/defaultBlockGroup").Task;
            
            foreach (BlockConfig configBlock in config.blocks)
            {
                for (int i = 0; i < configBlock.weight; i++)
                {
                    typePool.Add(configBlock.type);
                }
            }
            
            size = config.size;
            blocksMap = new NativeHashMap<int2, Entity>(config.size.x * config.size.y, Allocator.Persistent);
            await LoadConfigs(config.blocks);
            AddGroup(config.initialDepth);

            while (cam == null)
            {
                await Task.Yield();
            }


            
            IsReady = true;
        }

        public bool ContainsPoint(int2 point) => BlockUtil.ContainsPoint(point, size);

        public float3 ToWorldPoint(int2 blockPoint, float height) =>
            visualOrigin + new float3(blockPoint.x, height, blockPoint.y) * config.blockSize;
        
        public Entity GetBlock(int2 point) => blocksMap[point];

        protected override void OnDestroy()
        {
            blocksMap.Dispose();
        }

        protected override async Task LoadConfigs(BlockConfig[] configs)
        {
            visualOrigin = GameObject.Find("Origin").transform.position;
            await base.LoadConfigs(configs);
        }

        public int2? ScreenToBlockPoint(int level)
        {
            var plane = new Plane(Vector3.up, (Vector3)visualOrigin + new Vector3(0, -level + 0.5f, 0) * config.blockSize);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enterDistance))
            {
                float3 worldPoint = ray.GetPoint(enterDistance);
                float3 result = worldPoint - visualOrigin;

                result /= config.blockSize;
                var point = new int2(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.z));

                if (ContainsPoint(point) == false)
                {
                    return null;
                }

                return point;
            }

            return null;
        }

        public Entity CreateBlock(BlockType type, int2 position)
        {
            float blockSize = config.blockSize;
            Entity entity = CreateBaseEntity(visualOrigin + new float3(position.x, 0, position.y) * blockSize);
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = new float3(blockSize, 20, blockSize) });
            EntityManager.AddComponentData(entity, new Block() { Type = type });
            EntityManager.AddComponentData(entity, new BlockPoint() { Value = position });
            EntityManager.AddComponentData(entity, new Depth() { Value = 0 });
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            
            return entity;
        }
        
        private void AddGroup(int depth)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++) 
                {
                    var point = new int2(x, z);
                    BlockType type = typePool[random.NextInt(0, typePool.Count - 1)];
                    blocksMap.Add(point, CreateBlock(type, point));
                }
            }
        }
        
        protected override void OnUpdate()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

        }
    }
}