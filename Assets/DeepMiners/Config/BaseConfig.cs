using Unity.Rendering;
using UnityEngine;

namespace DeepMiners.Config
{
    public abstract class BaseConfig : ScriptableObject
    {
        public abstract int TypeIndex { get; }
        public abstract RenderMeshDescription GetDescription();
    }
}