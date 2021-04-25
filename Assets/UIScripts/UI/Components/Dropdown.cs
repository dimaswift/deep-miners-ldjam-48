using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class Dropdown : MonoBehaviour
    {
        [SerializeField] private DropdownOption optionPrefab;
        [SerializeField] private GameObject container;
        
        private readonly List<DropdownOption> optionsElementsBuffer = new List<DropdownOption>();

        private void Awake()
        {
            optionPrefab.gameObject.SetActive(false);
            GetComponent<Button>().onClick.AddListener(() =>
            {
                container.SetActive(!container.activeSelf);
            });
            container.SetActive(false);
        }

        public async Task SetUp(IEnumerable<(string action, object context, bool selected, Func<object, Task> callback)> items)
        {
            foreach (DropdownOption dropdownOption in optionsElementsBuffer)
            {
                dropdownOption.gameObject.SetActive(false);
                dropdownOption.SetSelected(false);
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
                optionElement.SetSelected(item.selected);
                await optionElement.SetUp(item.context, item.action, async e =>
                {
                    foreach (DropdownOption other in optionsElementsBuffer)
                    {
                        other.SetSelected(other == optionElement);
                    }
                    await item.callback(optionElement.Context);
                    
                }, null);
                index++;
            }
            
        }
    }
}