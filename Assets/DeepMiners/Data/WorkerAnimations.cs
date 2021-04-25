using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DeepMiners.Data
{
    public struct WorkerAnimations : IComponentData
    {
        public BlobAssetReference<KeyframeBlobArray> Bounce;
    }
}