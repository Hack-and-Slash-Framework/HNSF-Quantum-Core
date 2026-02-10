using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    public unsafe partial class HNSFStateActionExternal : AssetObject
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateAction action;
    }
}