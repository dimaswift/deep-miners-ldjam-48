using System.Collections.Generic;
using System.Threading.Tasks;
using Systems;
using DeepMiners.Config;
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

        
        
        public WorkerConfig ActiveWorker { get; private set; }
        
        protected override async Task OnActivated()
        {
            await blockGroupSystem.Build(blockGroupSystem.DefaultGroupSize);
            await Window.Show();
            isReady = true;
        }

        public async Task UntilReady()
        {
            while (isReady == false)
            {
                await Task.Yield();
            }
        }

        public int2 SelectedBlockSize()
        {
            if (blockGroupSystem.GroupSize.x == 0)
            {
                return blockGroupSystem.DefaultGroupSize;
            }

            return blockGroupSystem.DefaultGroupSize;
        }
        
        public IEnumerable<int2> GetAvailableSizes()
        {
            yield return new int2(8, 8);
            yield return new int2(16, 16);
            yield return new int2(32, 32);
            yield return new int2(64, 64);
            yield return new int2(128, 128);
            yield return new int2(256, 256);
            yield return new int2(512, 512);
            yield return new int2(1024, 1024);
        }

        public IEnumerable<WorkerConfig> GetAvailableWorkers() => workerFactorySystem.GetWorkersConfigs();
        
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
            await SetActiveWorker(workerFactorySystem.DefaultWorker);
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

        public async Task SetSize(int2 size)
        {
            if (blockGroupSystem != null && blockGroupSystem.IsReady && size.Equals(blockGroupSystem.GroupSize))
            {
                return;  
            }
            
            Window.SetInteractable(false);
            await blockGroupSystem.Build(size);
            Window.SetInteractable(true);
        }

        public Task SetActiveWorker(WorkerConfig config)
        {
            ActiveWorker = config;
            return Task.CompletedTask;
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
                    workerFactorySystem.CreateWorker(ActiveWorker, c);
                }
            }
        }
    }
}