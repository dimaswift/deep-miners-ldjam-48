using DeepMiners.Data;
using DeepMiners.Prefabs;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Block")]
    public class BlockConfig : BaseConfig
    {
        public BlockType type;
        public RenderMeshPrefab meshPrefab;
        public override int TypeIndex => (int) type;
        public override RenderMeshDescription GetDescription() => meshPrefab.GetDescription(); 
    }
}