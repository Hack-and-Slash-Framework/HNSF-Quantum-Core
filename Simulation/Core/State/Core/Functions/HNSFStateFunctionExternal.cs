using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.functions
{
    public unsafe partial class HNSFStateFunctionExternal : AssetObject
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction function;
    }
}