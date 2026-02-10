using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class SetGotHitReaction : HNSFStateAction
    {
        public AssetRef<HNSFStateActionExternal> gotHitReactionAction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->whenGotHitReactionAction = gotHitReactionAction;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetGotHitReaction());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetGotHitReaction;
            t.gotHitReactionAction = gotHitReactionAction;
            return base.CopyTo(target);
        }
    }
}