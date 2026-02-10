#if HNSF_UMOD
using System;

namespace Quantum
{
    [Serializable]
    public class QuantumAssetObjectSourceUMod : QuantumAssetSourceUMod<Quantum.AssetObject>, IQuantumAssetObjectSource
    {
        public SerializableType<Quantum.AssetObject> SerializableAssetType;

        public QuantumAssetObjectSourceUMod()
        {
        }

        public QuantumAssetObjectSourceUMod(string modName, string path, Type assetType)
        {
            ResourceModName = modName;
            ResourcePath = path;
            SerializableAssetType = assetType;
        }

        public Type AssetType => SerializableAssetType;
    }
}
#endif