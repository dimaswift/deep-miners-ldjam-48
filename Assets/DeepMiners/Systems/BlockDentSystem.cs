﻿using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public static class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        DefaultWorldInitialization.Initialize("Default World", false);
#endif
        }
    }
    
    [UpdateAfter(typeof(BlockGroupSystem))]
    public class BlockDentSystem : SystemBase
    {
        private float blockSize;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            blockSize = World.GetOrCreateSystem<BlockGroupSystem>().BlockSize;
        }

        protected override void OnUpdate()
        {
            float size = blockSize;
            EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Entities.WithNone<DestroyBlock>().ForEach((Entity entity, ref Translation translation,
                in Dent dent, in BlockPoint point, in BlockGroupVisualOrigin origin) =>
            {
                float y = origin.Value.y - point.Value.y;
                translation.Value = new float3(translation.Value.x, -dent.Value, translation.Value.z);

            }).Schedule(); 
        }
    }
}