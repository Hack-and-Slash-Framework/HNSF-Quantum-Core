using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HNSFNOTDecision : HNSFStateDecision
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision decision;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return !decision.Decide(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HNSFNOTDecision());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HNSFNOTDecision;
            t.decision = decision.Copy();
            return base.CopyTo(target);
        }
    }
}