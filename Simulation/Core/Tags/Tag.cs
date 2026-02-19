using System.Collections.Generic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class Tag : AssetObject
    {
        public List<AssetRef<Tag>> allParents = new List<AssetRef<Tag>>();
        public AssetRef<Tag> parent;
        
        public string label;
#if QUANTUM_UNITY
        [TextArea]
#endif
        public string description;

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