using System.Threading.Tasks;
using DeepMiners.Systems.Input.Windows;
using DeepMiners.UI.Systems;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class MainWindow : Window<WelcomeScreenInputSystem>, IMainWindow
    {
        [SerializeField] private Button sandboxButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI versionText;
        
        protected override Task OnInit()
        {
            Bind(sandboxButton, async () => await System.DispatchSandboxClick());
            Bind(quitButton, Application.Quit);
            versionText.text = Application.version;
            return base.OnInit();
        }
    }
}