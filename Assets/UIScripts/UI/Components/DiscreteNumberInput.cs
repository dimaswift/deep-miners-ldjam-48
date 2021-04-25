using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    public class DiscreteNumberInput : CustomProperty<int>
    {
        [SerializeField] private Button plus;
        [SerializeField] private Button minus;

        private int min = 0, max = int.MaxValue;

        public override void SetBounds(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
        
        protected override bool ValidateValue(ref int v)
        {
            base.ValidateValue(ref v);
            v = Mathf.Clamp(v, min, max);
            return true;
        }

        private void Awake()
        {
            plus.onClick.AddListener(() =>
            {
                Value++;
            });
            minus.onClick.AddListener(() =>
            {
                Value--;
            });
            
        }
    }
}