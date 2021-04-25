using System;
using UnityEngine;

namespace DeepMiners.UI
{
    public class Element : MonoBehaviour
    {
        public Func<object> ContextHandler { get; private set; }
        
        public void SetContext(Func<object> context)
        {
            ContextHandler = context;
            OnContextChanged(context);
        }
        
        protected virtual void OnContextChanged(Func<object> context) {}
    }
}