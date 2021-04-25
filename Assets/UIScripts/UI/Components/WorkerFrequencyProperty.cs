using DeepMiners.Config;
using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.UI
{
    public class WorkerFrequencyProperty : WorkerPropertyWrapper
    {
        [SerializeField] private FloatProperty input;

        private const float MAX = 2;
        private const float MIN = 0.02f;
        
        protected override void OnWorkerAssigned(WorkerConfig config)
        {
            input.SetBounds(MIN, MAX);
            input.SetInitialValue(math.remap(MIN, MAX, MAX, MIN, config.frequency));
        }

        private void Start()
        {
            AddProperty(input, (value, worker) =>
            {
                worker.frequency = math.remap(MAX, MIN, MIN, MAX, value);
            });
        }
    }
}