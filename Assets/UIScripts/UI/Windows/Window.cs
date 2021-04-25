using System;
using System.Threading.Tasks;
using DeepMiners.Systems.Input;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public abstract class Window<T> : Window where T : SystemBase
    {
        protected override async Task OnInit()
        {
            while (System == null)
            {
                await Task.Yield();
            }
        }

        protected T System => World.DefaultGameObjectInjectionWorld.GetExistingSystem<T>();
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : MonoBehaviour, IWindow
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform root;
        [SerializeField] private float animationScale = 0.2f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected void Bind(Button button, Func<Task> task)
        {
            button.onClick.AddListener(async () => await task());
        }
        
        protected void Bind(Button button, UnityAction action)
        {
            button.onClick.AddListener(action);
        }
        
        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && closeButton != null && IsShown)
            {
                await Hide();
            }
        }

        private async void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);

            if (root == null)
            {
                root = transform.Find("Root")?.GetComponent<RectTransform>();
                if (root == null)
                {
                    root = transform.GetChild(0)?.GetComponent<RectTransform>();
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(async () => await Hide());
            }
            
            await OnInit();
            IsInitialized = true;
        }
        
        private void OnValidate()
        {
            root = transform.Find("Root")?.GetComponent<RectTransform>();
            if (closeButton == null)
            {
                closeButton = root?.Find("CloseButton")?.GetComponent<Button>();
            }
          
        }

        public async void Dispose()
        {
            await OnDispose();
            Destroy(gameObject);
        }

        protected virtual Task OnDispose()
        {
            return Task.CompletedTask;
        }

        [SerializeField] private float animationTime = 0.3f;

        public bool IsShown { get; private set; }

        public bool IsInitialized { get; private set; }

        private CanvasGroup canvasGroup;


        protected virtual Task OnInit()
        {
            return Task.CompletedTask;
        }

        protected virtual async Task ShowAnimation()
        {
            canvasGroup.alpha = 0;
            float t = 0f;
            while (t < 1)
            {
                canvasGroup.alpha = animationCurve.Evaluate(t);
                root.localScale = Vector3.Lerp(Vector3.one * (1 + animationScale), Vector3.one,
                    animationCurve.Evaluate(t));
                t += Time.unscaledDeltaTime / animationTime;
                await Task.Yield();
            }

            root.localScale = Vector3.one;
        }

        protected virtual async Task HideAnimation()
        {
            float t = 0f;
            while (t < 1)
            {
                root.localScale = Vector3.Lerp(Vector3.one,
                    Vector3.one * (1 + animationScale), animationCurve.Evaluate(t));
                canvasGroup.alpha = 1 - t;
                t += Time.unscaledDeltaTime / animationTime;
                await Task.Yield();
            }
        }

        public async Task Show()
        {
            gameObject.SetActive(true);
            await ShowAnimation();
            await OnShown();
            SetInteractable(true);
            IsShown = true;
        }

        public void SetInteractable(bool interactable) => canvasGroup.interactable = interactable;

        public async Task Hide()
        {
            SetInteractable(false);
            await HideAnimation();
            await OnHidden();
            gameObject.SetActive(false);
            IsShown = false;
        }

        protected virtual Task OnShown()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnHidden()
        {
            return Task.CompletedTask;
        }
    }
}