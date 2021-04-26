using System.Threading.Tasks;
using DeepMiners.Data;
using DeepMiners.Extensions;
using DeepMiners.Prefabs;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Worker")]
    public class WorkerConfig : BaseConfig
    {
        public string displayName = "Worker";
        public WorkerAbility ability;
        public override int TypeIndex => (int) ability;
        public float power = 0.5f;
        public float frequency = 0.5f;

        public float size = 0.5f;
        public Color Color { get; set; }

        public float sizeLossPerHit = 0.01f;

        public int radius = 1;
        public int maxBounces = 3;
        public float markDuration = 1;
    }
}