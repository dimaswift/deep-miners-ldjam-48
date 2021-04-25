using System;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class FloatProperty : CustomProperty<float>
    {
        [SerializeField] private Slider input;

        public override void SetInitialValue(float value)
        {
            base.SetInitialValue(value);
            input.value = value;
        }

        private void Start()
        {
            input.onValueChanged.AddListener(v => Value = v);
        }
        
        protected override string FormatValue(float v)
        {
            return v.ToString("F100");
        }

        public override void SetBounds(float min, float max)
        {
            input.minValue = min;
            input.maxValue = max;
        }

        protected override bool ValidateValue(ref float v)
        {
            return true;
        }
    }
}