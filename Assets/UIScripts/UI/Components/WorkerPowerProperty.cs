using DeepMiners.Config;
using UnityEngine;

namespace DeepMiners.UI
{
    public class WorkerPowerProperty : WorkerPropertyWrapper
    {
        [SerializeField] private FloatProperty input;

        private const float MAX = 1;
        private const float MIN = 0.005f;
        
        protected override void OnWorkerAssigned(WorkerConfig config)
        {
            input.SetBounds(MIN, MAX);
            input.SetInitialValue(config.power);
        }

        private void Start()
        {
            AddProperty(input, (value, worker) =>
            {
                worker.power = value;
            });
        }
    }
}