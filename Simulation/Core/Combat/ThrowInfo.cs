using HnSF.core.state.decisions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class ThrowInfo : AssetObject
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions;

        public FP maxDistance = -1;
        public int hitstop;
        public int throweeId;
    }
}
