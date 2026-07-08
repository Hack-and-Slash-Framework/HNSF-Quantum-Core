using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core
{
    [System.Serializable]
    public unsafe partial class HitResolvePairAction : AssetObject
    {
        public virtual void Resolve(ref HitResolvePairInfo pairInfo)
        {
            
        }
    }
}
