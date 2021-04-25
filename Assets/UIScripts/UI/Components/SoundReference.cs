using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepMiners.UI
{
    [Serializable]
    public class SoundReference : AssetReferenceT<AudioClip>
    {
        public SoundReference(string guid) : base(guid)
        {
        }
    }
}