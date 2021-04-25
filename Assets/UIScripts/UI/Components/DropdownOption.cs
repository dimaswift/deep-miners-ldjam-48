using System;
using System.Threading.Tasks;
using DeepMiners.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    [RequireComponent(typeof(Button))]
    public class DropdownOption : Element
    {
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject selectionFrame;

        private Func<DropdownOption, Task> callback;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(async () =>
            {
                button.interactable = false;
                await callback(this);
                button.interactable = true;
            });
        }

        public void SetSelected(bool selected)
        {
            selectionFrame.SetActive(selected);
        }

        public async Task SetOptions(string action, Func<DropdownOption, Task> callback, AssetReferenceSprite iconRef)
        {
            this.callback = callback;
            actionText.text = action;

            if (icon != null)
            {
                try
                {
                    if (iconRef != null && iconRef.RuntimeKeyIsValid())
                    {
                        var sprite = await iconRef.LoadOrGetAsync<Sprite>();
                        icon.sprite = sprite;
                    }
                    else
                    {
                        icon.gameObject.SetActive(false);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"cannot load icon: {iconRef?.SubObjectName}");
                    Debug.LogException(e);
                }
            }
        }
    }
}