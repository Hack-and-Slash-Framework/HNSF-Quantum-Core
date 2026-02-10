using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HNSFExternalDecision : HNSFStateDecision
    {
        public AssetRef<HNSFStateDecisionExternal> decision;
        [NonSerialized] private HNSFStateDecisionExternal _decision = null;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (_decision == null && !frame.TryFindAsset(decision, out _decision))
            {
                Log.Error("No decision asset found for external decision.");
                return false; 
            }
            return _decision.decision.Decide(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HNSFExternalDecision());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HNSFExternalDecision;
            t.decision = decision;
            return base.CopyTo(target);
        }
    }
}