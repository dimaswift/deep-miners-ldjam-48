using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Data;
using DeepMiners.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class BlockGroupSystem : ConfigurableSystem<BlockConfig>
    {

        public int2 DefaultGroupSize => config.size;
        
        public event Action OnWillBuild = () => { }; 
        public event Action OnBuilt = () => { }; 
        
        public float BlockScale => config.blockScale;
        
        public int2 GroupSize { get; private set; }
        
        public bool IsReady { get; private set; }
        
        public bool Initialized { get; private set; }
        
        public bool IsBuilding { get; private set; }
        
        public NativeHashMap<int2, Entity> BlocksMap => blocksMap;
        public float3 VisualOrigin => float3.zero;
        
        private BlockGroupConfig config;
        private Camera cam;
        private NativeHashMap<int2, Entity> blocksMap;

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

            await LoadConfigs(config.blocks);

            Initialized = true;
        }

        public bool ContainsPoint(int2 point) => BlockUtil.ContainsPoint(point, GroupSize);

        public float3 ToWorldPoint(int2 blockPoint, float height) =>
            VisualOrigin + new float3(blockPoint.x, height, blockPoint.y) * config.blockScale;
        
        public Entity GetBlock(int2 point) => blocksMap[point];

        protected override void OnDestroy()
        {
            if (blocksMap.IsCreated)
            {
                blocksMap.Dispose();
            }
        }

        protected override async Task LoadConfigs(BlockConfig[] configs)
        {
            await base.LoadConfigs(configs);
        }

        public int2? ScreenToBlockPoint(int level)
        {
            var plane = new Plane(Vector3.up, (Vector3)VisualOrigin + new Vector3(0, -level + 0.5f, 0) * config.blockScale);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enterDistance))
            {
                float3 worldPoint = ray.GetPoint(enterDistance);
                float3 result = worldPoint - VisualOrigin;

                result /= config.blockScale;
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
            float scale = config.blockScale;
            Entity entity = CreateBaseEntity(VisualOrigin + new float3(position.x, 0, position.y) * scale);
            EntityManager.AddComponentData(entity, new NonUniformScale() { Value = new float3(scale, GroupSize.x, scale) });
            EntityManager.AddComponentData(entity, new Block() { Type = type });
            EntityManager.AddComponentData(entity, new BlockPoint() { Value = position });
            EntityManager.AddComponentData(entity, new Depth() { Value = 0 });
            RenderMeshUtility.AddComponents(entity, EntityManager, MeshDescriptions[(int)type]);
            
            return entity;
        }

        public void CleanUp()
        {
            if (blocksMap.IsCreated)
            {
                EntityManager.DestroyAndResetAllEntities();
                
                blocksMap.Dispose();
            }

            IsReady = false;
        }
        
        public async Task Build(int2 size)
        {
            if (IsBuilding)
            {
                return;
            }

            GroupSize = size;
            
            OnWillBuild();

            IsBuilding = true;
            
            IsReady = false;
            
            var drill = World.GetOrCreateSystem<DrillSystem>();

            while (drill.CurrentJob.HasValue && drill.CurrentJob.Value.IsCompleted == false)
            {
                await Task.Yield();
            }
            
            await Task.Yield();

            CleanUp();

            blocksMap = new NativeHashMap<int2, Entity>(size.x * size.y, Allocator.Persistent);
            
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++) 
                {
                    var point = new int2(x, z);
                    blocksMap.Add(point, CreateBlock(BlockType.Dirt, point));
                }
                await Task.Yield();
            }
            
            IsReady = true;
            
            IsBuilding = false;

            OnBuilt();
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