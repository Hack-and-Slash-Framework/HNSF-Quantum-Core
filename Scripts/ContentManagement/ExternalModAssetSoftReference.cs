using Quantum;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Mod Asset Soft Reference")]
    public class ExternalModAssetSoftReference : ScriptableObject
    {
        public ModAssetSoftReference reference;
        public AssetRef quantumAssetReference;
    }
}