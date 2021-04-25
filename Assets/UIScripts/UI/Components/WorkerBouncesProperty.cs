using DeepMiners.Config;
using UnityEngine;

namespace DeepMiners.UI
{
    public class WorkerBouncesProperty : WorkerPropertyWrapper
    {
        [SerializeField] private DiscreteNumberInput input;

        protected override void OnWorkerAssigned(WorkerConfig config)
        {
            input.SetBounds(1, 100);
            input.SetInitialValue(config.maxBounces);
        }

        private void Start()
        {
            AddProperty(input, (value, worker) =>
            {
                worker.maxBounces = value;
            });
        }
    }
}