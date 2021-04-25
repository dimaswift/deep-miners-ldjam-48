using DeepMiners.Config;
using UnityEngine;

namespace DeepMiners.UI
{
    public class WorkerDurabilityProperty : WorkerPropertyWrapper
    {
        [SerializeField] private FloatProperty input;

        private const float MAX = 0.5f;
        
        protected override void OnWorkerAssigned(WorkerConfig config)
        {
            input.SetBounds(0, MAX);
            input.SetInitialValue(MAX - config.sizeLossPerHit);
        }

        private void Start()
        {
            AddProperty(input, (value, worker) =>
            {
                worker.sizeLossPerHit = MAX - value;
            });
        }
    }
}