using DeepMiners.Data;
using DeepMiners.Prefabs;
using Unity.Rendering;
using UnityEngine;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Worker")]
    public class WorkerConfig : BaseConfig
    {
        public WorkerType type;
        
        public RenderMeshPrefab prefab;
        public override int TypeIndex => (int) type;
        public override RenderMeshDescription GetDescription() => prefab.GetDescription();
    }
}