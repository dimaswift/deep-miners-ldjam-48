using System;
using System.Threading.Tasks;
using DeepMiners.Data;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class GameModeSelectionSystem : SystemBase
    {
        public event Action<GameMode> OnModeChanged = mode => { }; 
        public event Action<GameMode, GameMode> OnWillChangeMode = (old, n) => { }; 
        
        public bool IsChangingMode { get; private set; }
        
        public GameMode CurrentMode { get; private set; }

        private BlockGroupSystem blockGroupSystem;

        public Task SetCurrentMode(GameMode mode)
        {
            if (IsChangingMode)
            {
                return Task.CompletedTask;
            }

            if (CurrentMode == mode)
            {
                return Task.CompletedTask;;
            }

            OnWillChangeMode(CurrentMode, mode);
            
            IsChangingMode = true;

            if (blockGroupSystem.IsReady)
            {
                blockGroupSystem.CleanUp();
            }

            CurrentMode = mode;
            
            IsChangingMode = false;

            OnModeChanged(mode);

            return Task.CompletedTask;
        }

        protected override void OnCreate()
        {
            blockGroupSystem = World.GetOrCreateSystem<BlockGroupSystem>();
            SetCurrentMode(GameMode.MainMenu);
        }

        protected override void OnUpdate()
        {
            
        }
    }
}