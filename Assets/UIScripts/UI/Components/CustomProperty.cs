using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace DeepMiners.UI
{
    public abstract class CustomProperty<T> : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI valueText;

        public T Value
        {
            get => value;
            protected set
            {
                if (ValidateValue(ref value))
                {
                    this.value = value;
                    OnChanged(value);
                }
                
            }
        }
        
        public virtual void SetBounds(T min, T max)
        {
            
        }
        
        public virtual void SetInitialValue(T value)
        {
            ValidateValue(ref value);
            this.value = value;
        }

        protected virtual bool ValidateValue(ref T v)
        {
            if (valueText != null)
            {
                valueText.text = FormatValue(v);
            }
            return true;
        }

        protected virtual string FormatValue(T v) => v.ToString();
        
        public event UnityAction<T> OnChanged = v => { };
        
        [NonSerialized]
        private T value;
    }
}