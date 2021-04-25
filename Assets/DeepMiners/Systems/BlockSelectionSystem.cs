using System.Threading.Tasks;
using DeepMiners.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(WorkerMoveSystem))]
    public class BlockSelectionSystem : SystemBase
    {
        private BlockGroupSystem blockGroupSystem;
        private WorkerFactorySystem workerFactorySystem;

        private bool isReady;
        private double lastWorkerSpawn;
        private double currentSpawnRate = 0.01f;
        
        protected override async void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            workerFactorySystem = World.GetExistingSystem<WorkerFactorySystem>();
           
            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            isReady = true;
        }
        
        protected override void OnUpdate()
        {
            if (!isReady)
            {
                return;
            }

           
            
            if (Input.GetMouseButton(0))
            {
                if (Time.ElapsedTime - lastWorkerSpawn < currentSpawnRate)
                {
                    return;
                }
                
                lastWorkerSpawn = Time.ElapsedTime;

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