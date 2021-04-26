using System.Threading.Tasks;
using DeepMiners.Extensions;
using DeepMiners.Prefabs;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepMiners.Config
{
    public abstract class BaseConfig : ScriptableObject
    {
        public abstract int TypeIndex { get; }
        public AssetReferenceGameObject prefab;
        public async Task<RenderMeshDescription> GetDescription()
        {
           var p = await prefab.LoadOrGetComponentAsync<RenderMeshPrefab>();
           return p.GetDescription();
        }
    }
}