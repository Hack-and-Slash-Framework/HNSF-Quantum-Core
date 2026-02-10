using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HNSFANDDecision : HNSFStateDecision
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] decisions = Array.Empty<HNSFStateDecision>();

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            for (int i = 0; i < decisions.Length; i++)
            {
                if (decisions[i] == null) continue;
                if (!decisions[i].Decide(frame, entity, ref stateContext)) return false;
            }

            return true;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HNSFANDDecision());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HNSFANDDecision;
            t.decisions = new HNSFStateDecision[decisions.Length];

            for (int i = 0; i < decisions.Length; i++)
            {
                t.decisions[i] = decisions[i].Copy();
            }
            return base.CopyTo(target);
        }
    }
}