using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class Dropdown : Element
    {
        [SerializeField] private Element[] subElements;
        [SerializeField] private DropdownOption optionPrefab;
        [SerializeField] private GameObject container;
        
        private readonly List<DropdownOption> optionsElementsBuffer = new List<DropdownOption>();

        private void Awake()
        {
            if (optionPrefab != null)
            {
                optionPrefab.gameObject.SetActive(false);
            }
            GetComponent<Button>().onClick.AddListener(() =>
            {
                container.SetActive(!container.activeSelf);
            });
            container.SetActive(false);
        }

        protected override void OnContextChanged(Func<object> context)
        {
            foreach (Element subElement in subElements)
            {
                subElement.SetContext(context);
            }
        }

        public async Task AddOptions(IEnumerable<(string action, Func<object> context, bool selected, Func<object, Task> callback)> items)
        {
            if (optionPrefab == null)
            {
                Debug.LogError($"Option Prefab is missing on {name}");
                return;
            }

            int index = 0;
            
            foreach (var item in items)
            {
                DropdownOption optionElement;
                if (index >= optionsElementsBuffer.Count)
                {
                    optionElement = Instantiate(optionPrefab);
                    optionElement.transform.SetParent(optionPrefab.transform.parent);
                    optionElement.transform.localScale = Vector3.one;
                    optionElement.gameObject.SetActive(true);
                    optionsElementsBuffer.Add(optionElement);
                }
                else
                {
                    optionElement = optionsElementsBuffer[index];
                }
                optionElement.SetContext(item.context);
                optionElement.SetSelected(item.selected);
                await optionElement.SetOptions(item.action, async e =>
                {
                    foreach (DropdownOption other in optionsElementsBuffer)
                    {
                        other.SetSelected(other == optionElement);
                    }
                    await item.callback(optionElement.ContextHandler());
                    
                }, null);
                index++;
            }
            
        }
    }
}