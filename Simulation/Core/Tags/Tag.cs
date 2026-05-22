using System.Collections.Generic;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class Tag : AssetObject
    {
        public List<AssetRef<Tag>> allParents = new List<AssetRef<Tag>>();
        public AssetRef<Tag> parent;
        public List<AssetRef<Tag>> childTags = new List<AssetRef<Tag>>();
        
        public string label;
#if QUANTUM_UNITY
        [TextArea]
#endif
        public string description;

        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(label);
        }
        
#if UNITY_EDITOR
        public void OnValidate()
        {
            if (Application.isPlaying) return;
            allParents.Clear();
            if (parent == default || parent == this) return;

            var nextParent = parent;
            while (nextParent != null)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(nextParent, out var nextParentAsset)) break;
                allParents.Add(nextParentAsset);
                nextParent = nextParentAsset.parent;
            }
        }
#endif
    }
}