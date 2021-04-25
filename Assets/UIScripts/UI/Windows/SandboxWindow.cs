using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Config;
using DeepMiners.Systems.Input;
using DeepMiners.Systems.Input.Windows;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class SandboxWindow : Window<SandboxInputSystem>, ISandboxWindow
    {
        [SerializeField] private Button resetButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsWindow settingsWindow;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Dropdown presetDropdown;
        [SerializeField] private Dropdown customWorkerProperties;
        
        
        protected override async Task OnInit()
        {
            await base.OnInit();
            await System.UntilReady();
            Bind(resetButton, System.Reset);
            Bind(settingsButton, async () => await settingsWindow.Show());
            await resolutionDropdown.AddOptions(GenerateResolutionDropdowns());
            await presetDropdown.AddOptions(GeneratePresetDropdowns());
            customWorkerProperties.SetContext(System.GetCustomWorker);
        }
        
        private IEnumerable<(string action, Func<object> context, bool selected, Func<object, Task> callback)> GenerateResolutionDropdowns ()
        {
            foreach (int2 size in System.GetAvailableSizes())
            {
                yield return ($"{size.x}x{size.y}", () => size, size.Equals(System.SelectedBlockSize()), s => System.SetSize((int2)s));
            }
        }
        
        private IEnumerable<(string action, Func<object> context, bool selected, Func<object, Task> callback)> GeneratePresetDropdowns ()
        {
            foreach (WorkerConfig worker in System.GetAvailableWorkers())
            {
                yield return ($"{worker.displayName}", () => worker, worker == System.ActiveWorker, async s =>
                {
                    await System.SetActiveWorker((WorkerConfig) s);
                    customWorkerProperties.gameObject.SetActive(System.IsCustomWorkerSelected());
                });
            }
        }
    }
}