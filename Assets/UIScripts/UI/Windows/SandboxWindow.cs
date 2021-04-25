using System.Threading.Tasks;
using DeepMiners.Systems.Input;
using DeepMiners.Systems.Input.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class SandboxWindow : Window<SandboxInputSystem>, ISandboxWindow
    {
        [SerializeField] private Button resetButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsWindow settingsWindow;
        
        protected override Task OnInit()
        {
            Bind(resetButton, System.Reset);
            Bind(settingsButton, async () => await settingsWindow.Show());
            return base.OnInit();
        }
    }
}