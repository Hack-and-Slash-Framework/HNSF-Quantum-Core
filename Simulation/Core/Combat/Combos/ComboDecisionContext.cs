using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.GroupControl.Combo
{
    public unsafe partial struct ComboDecisionContext
    {
        public AIComboMemory* comboMemory;
        public byte comboIndex;
        public ComboBattleScript currentlyEvaluating;
    }
}