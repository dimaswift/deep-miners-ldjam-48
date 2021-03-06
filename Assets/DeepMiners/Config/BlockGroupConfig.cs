using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.Config
{
    [CreateAssetMenu(menuName = "DeepMiners/Configs/Block Group")]
    public class BlockGroupConfig : ScriptableObject
    {
        public float blockScale = 1f;
        public int2 size = new int2() { x = 10, y = 100 };
        public BlockConfig[] blocks;
    }
}