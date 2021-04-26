using DeepMiners.Prefabs;
using UnityEngine;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Drill")]
    public class DrillConfig : ScriptableObject
    {
        public WorkerConfig[] workers;
        public AnimationCurve drillBounceCurve;
        public AnimationCurve drillMoveCurve;
        public RenderMeshPrefab hitEffect;
        public AudioClip[] hitSounds;
    }
}