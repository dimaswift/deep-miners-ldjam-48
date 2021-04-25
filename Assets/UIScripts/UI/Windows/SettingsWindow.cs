using System.Threading.Tasks;
using Systems;
using DeepMiners.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class SettingsWindow : Window<GameModeSelectionSystem>
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;


        protected override Task OnInit()
        {
            Bind(mainMenuButton, async () =>
            {
                await Hide();
                await System.SetCurrentMode(GameMode.None);
            });
            Bind(quitButton, Application.Quit);
            return base.OnInit();
        }
        
        protected override Task OnShown()
        {
            Time.timeScale = 0;
            return base.OnShown();
        }

        protected override Task OnHidden()
        {
            Time.timeScale = 1;
            return base.OnHidden();
        }
    }
}