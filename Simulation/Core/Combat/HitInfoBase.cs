using System.Collections.Generic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public unsafe partial class HitInfoBase : AssetObject
    {
#if QUANTUM_UNITY
        [Header("Base Properties")]
#endif
        public List<AssetRef<Tag>> attributes;
        public int clashLevel;
        public bool dontClash;
        public AssetRef<HitInfoBase> counterhitInfo;
        public int hitCount = 1;

        public virtual void OnValidate()
        {
            
        }
    }
}
