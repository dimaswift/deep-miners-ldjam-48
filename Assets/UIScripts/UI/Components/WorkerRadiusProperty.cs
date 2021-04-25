using UnityEngine;

namespace DeepMiners.UI
{
    public class WorkerRadiusProperty : WorkerPropertyWrapper
    {
        [SerializeField] private DiscreteNumberInput input;

        private void Start()
        {
            input.SetBounds(0, 5);
            input.SetInitialValue(0);
            AddProperty(input, (value, worker) =>
            {
                worker.radius = value;
            });
        }
    }
    
    
}