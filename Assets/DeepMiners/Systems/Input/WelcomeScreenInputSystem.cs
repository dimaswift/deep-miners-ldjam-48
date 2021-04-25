using System.Threading.Tasks;
using Systems;
using DeepMiners.Data;
using DeepMiners.Systems.Input;
using DeepMiners.Systems.Input.Windows;
using Unity.Entities;
using UnityEngine;

namespace DeepMiners.UI.Systems
{
    [UpdateInGroup(typeof (LateSimulationSystemGroup), OrderLast = true)]
    public class WelcomeScreenInputSystem : BaseModeSystem<IMainWindow>
    {
        protected override GameMode Mode => GameMode.None;

        protected override void OnUpdate()
        {
            
        }

        protected override async Task OnActivated()
        {
            await Task.Delay(1000);
            await Window.Show();
        }

        protected override async Task OnInit()
        {
            await Task.Delay(1000);
            await Window.Show();
        }

        public async Task DispatchSandboxClick()
        {
            await Window.Hide();
            await SetGameMode(GameMode.Sandbox);
        }
    }
}