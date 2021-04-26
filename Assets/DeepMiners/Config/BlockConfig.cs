using DeepMiners.Data;
using DeepMiners.Prefabs;
using UnityEngine;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Block")]
    public class BlockConfig : BaseConfig
    {
        public BlockType type;
        public RenderMeshPrefab meshPrefab;
        public override int TypeIndex => (int) type;
        public int weight = 1;
    }
}