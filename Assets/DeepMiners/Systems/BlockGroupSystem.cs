using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
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
        public NativeHashMap<int3, Entity> BlocksMap => blocksMap;
        public float3 VisualMineOrigin => visualMineOrigin;
        
        private BlockGroupConfig config;
        private Camera cam;
        private Random random = Random.CreateFromIndex(0);
        private float3 visualMineOrigin;
        private NativeHashMap<int3, Entity> blocksMap;
        
        protected override async void OnCreate()
        {
            cam = Camera.main;
           
            config = await Addressables.LoadAssetAsync<BlockGroupConfig>("configs/mine").Task;
            blocksMap = new NativeHashMap<int3, Entity>(config.maxDepth * config.size.x * config.size.x,
                Allocator.Persistent);
            await LoadConfigs(config.blocks);
            CreateGroup(config.initialDepth, 0);
        }

        public Entity GetBlock(int3 point) => blocksMap[point];

        protected override void OnDestroy()
        {
            blocksMap.Dispose();
        }

        protected override async Task LoadConfigs(BlockConfig[] configs)
        {
            visualMineOrigin = GameObject.Find("MineOrigin").transform.position;
            await base.LoadConfigs(configs);
        }

        public int3? ScreenToMinePoint(int level)
        {
            var plane = new Plane(Vector3.up, new Vector3(0, -level, 0));
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enterDistance))
            {
                float3 worldPoint = ray.GetPoint(enterDistance);
                float3 result = visualMineOrigin - worldPoint;
                return new int3(Mathf.FloorToInt(result.x), level, Mathf.FloorToInt(result.z));
            }

            return null;
        }

        public Entity CreateBlock(BlockType type, int3 position)
        {
            Entity entity = CreateBaseEntity(visualMineOrigin + position);

            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = new float3(1, 1, 1) });
            EntityManager.AddComponentData(entity, new Block() { Type = type });
            EntityManager.AddComponentData(entity, new BlockPoint() { Value = position });
            EntityManager.AddComponentData(entity, new BlockGroupVisualOrigin() { Value = visualMineOrigin });
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            
            return entity;
        }
        
        private void CreateGroup(int depth, int levelOffset)
        {
            int2 size = config.size;

            for (int level = levelOffset; level < levelOffset + depth; level++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int z = 0; z < size.x; z++)
                    {
                        var point = new int3(x, -level, z);
                        BlockType type = (BlockType) random.NextInt(0, (int) BlockType.OresAmount);
                        blocksMap.Add(point, CreateBlock(type, point));
                    }
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