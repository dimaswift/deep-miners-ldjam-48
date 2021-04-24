using UnityEngine;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Drill")]
    public class DrillConfig : ScriptableObject
    {
        public WorkerConfig[] workers;
    }
}