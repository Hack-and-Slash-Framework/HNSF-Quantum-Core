using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.GroupControl.Combo.Considerations
{
    [System.Serializable]
    public class AIComboConsideration
    {
        public FP Calculate(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            return 1;
        }
    }
}