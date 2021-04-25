using System;
using System.Threading.Tasks;
using Systems;
using DeepMiners.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace DeepMiners.Systems.Input
{
    public abstract class BaseModeSystem<T> : SystemBase where T : IWindow
    {
        protected abstract GameMode Mode { get; }
        
        protected virtual Task OnActivated () => Task.CompletedTask;
        protected virtual Task OnDeactivated () => Task.CompletedTask;
        protected virtual Task OnWillBeDeactivated () => Task.CompletedTask;
        
        private GameModeSelectionSystem gameModeSelectionSystem;

        protected T Window { get; private set; }

        protected async Task SetGameMode(GameMode mode)
        {
            await gameModeSelectionSystem.SetCurrentMode(mode);
        }
        
        protected sealed override async void OnCreate()
        {
            await Task.Yield();
            
            string path = $"Assets/Windows/{typeof(T).Name.Substring(1, typeof(T).Name.Length - 1)}.prefab";

            try
            {
                GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(path).Task;
                GameObject instance = Object.Instantiate(prefab);
                Window = instance.GetComponent<T>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Cannot load window for system {typeof(T).Name}: " + e.Message);
            }

            gameModeSelectionSystem = World.GetOrCreateSystem<GameModeSelectionSystem>();
            gameModeSelectionSystem.OnModeChanged += OnModeChanged;
            gameModeSelectionSystem.OnWillChangeMode += OnWillChangeMode;

            await OnInit();
        }
        
        protected virtual Task OnInit() => Task.CompletedTask;

        private async void OnWillChangeMode(GameMode old, GameMode newMode)
        {
            if (old == Mode)
            {
                await OnWillBeDeactivated();
            }
        }
        
        private async void OnModeChanged(GameMode mode)
        {
            if (mode == Mode)
            {
                await OnActivated();
            }
            else
            {
                await OnDeactivated();
            }
        }


        protected override void OnUpdate()
        {
            
        }
    }
}