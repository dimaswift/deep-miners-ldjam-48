using Systems;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.Scene
{
    public class CameraSystem : SystemBase
    {
        private Camera cam;
        private Transform camTransform;
        private NativeArray<float> minDepthResult;
        private CameraSetup setup;
        private EntityQuery query;
        private float targetZoom;
        private BlockGroupSystem blockGroupSystem;

        private float targetDepth;
        private Vector3 pressedCamPos;
        private Vector3 pressedScreenPos;

        private double lastDepthUpdateTime;
        
        protected override void OnCreate()
        {
            blockGroupSystem = World.GetExistingSystem<BlockGroupSystem>();
            minDepthResult = new NativeArray<float>(1, Allocator.Persistent);
            query = GetEntityQuery(typeof(Block), typeof(Depth));
        }

        protected override void OnDestroy()
        {
            minDepthResult.Dispose();
        }

        protected override void OnUpdate()
        {
            if (cam == null)
            {
                cam = Camera.main;
                if (cam == null)
                {
                    return;
                }

                targetZoom = cam.orthographicSize;
                camTransform = cam.transform;
                setup = camTransform.GetComponent<CameraSetup>();
            }

            var depths = query.ToComponentDataArray<Depth>(Allocator.TempJob);

            var result = minDepthResult;
            
            Job.WithCode(() =>
            {
                float average = 0;
                for (int i = 0; i < depths.Length; i++)
                {
                    float d = depths[i].Value;
                    average += d;
                }
                result[0] = average / depths.Length;
                
            }).Run();

            if (Time.ElapsedTime - lastDepthUpdateTime > setup.refreshRate)
            {
                lastDepthUpdateTime = Time.ElapsedTime;
                targetDepth = minDepthResult[0];
            }

            Vector3 pos = camTransform.position;
            pos.y = -targetDepth + setup.depthOffset;
            camTransform.position = Vector3.Lerp(camTransform.position, pos, Time.DeltaTime * setup.moveSpeed);
            
            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
            {
                targetZoom = Mathf.Clamp(targetZoom - Input.mouseScrollDelta.y * setup.zoomSpeed, setup.minZoom,
                    setup.maxZoom);
            }
            
            if (Mathf.Approximately(cam.orthographicSize, targetZoom) == false)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, UnityEngine.Time.deltaTime * 5);
            }
        }
    }
}
