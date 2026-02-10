using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.decisions
{
    public unsafe partial class HNSFStateDecisionExternal : AssetObject
    {
        #if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
        #endif
        public HNSFStateDecision decision;
    }
}
