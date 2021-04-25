using System.Threading.Tasks;
using Systems;
using DeepMiners.Data;
using DeepMiners.Systems.Input.Windows;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.EventSystems;

namespace DeepMiners.Systems.Input
{
    [UpdateInGroup(typeof (LateSimulationSystemGroup), OrderLast = true)]
    public class SandboxInputSystem : BaseModeSystem<ISandboxWindow>
    {
        private BlockGroupSystem blockGroupSystem;
        private WorkerFactorySystem workerFactorySystem;

        private bool isReady;
        private double lastWorkerSpawn;
        private double currentSpawnRate = 0.01f;

        protected override GameMode Mode => GameMode.Sandbox;
        
        protected override async Task OnActivated()
        {
            await blockGroupSystem.Build(blockGroupSystem.DefaultGroupSize);
            await Window.Show();
            isReady = true;
        }

        protected override async Task OnWillBeDeactivated()
        {
            await Window.Hide();
            isReady = false;
        }

        protected override async Task OnInit()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            workerFactorySystem = World.GetExistingSystem<WorkerFactorySystem>();
           
            while (blockGroupSystem.IsReady == false)
            {
                await Task.Yield();
            }
            isReady = true;
        }
        
        public async Task Reset()
        {
            var system = World.GetExistingSystem<BlockGroupSystem>();
            int2 size = system.GroupSize;
            await system.Build(size);
        }

        protected override void OnUpdate()
        {
            if (!isReady || !blockGroupSystem.IsReady)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject() == false)
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