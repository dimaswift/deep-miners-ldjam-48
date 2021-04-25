using System;
using DeepMiners.Config;
using TMPro;
using UnityEngine;

namespace DeepMiners.UI
{
    public abstract class WorkerPropertyWrapper : Element
    {
        protected virtual void OnWorkerAssigned(WorkerConfig config)
        {
            
        }

        protected override void OnContextChanged(Func<object> context)
        {
            OnWorkerAssigned(context() as WorkerConfig);
        }

        protected void AddProperty<T>(CustomProperty<T> property, Action<T,WorkerConfig> writer)
        {
            property.OnChanged += v =>
            {
                writer(v, ContextHandler() as WorkerConfig);
            };
        }
    }
}