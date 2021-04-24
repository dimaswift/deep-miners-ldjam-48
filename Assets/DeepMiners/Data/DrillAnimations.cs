using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DeepMiners.Data
{
    public struct DrillAnimations : IComponentData
    {
        public BlobAssetReference<KeyframeBlobArray> Bounce;
    }
}