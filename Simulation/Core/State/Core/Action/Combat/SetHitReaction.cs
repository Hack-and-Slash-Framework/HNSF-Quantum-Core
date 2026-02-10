using System;
using HnSF.core.state.functions;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class SetHitReaction : HNSFStateAction
    {
        public AssetRef<HNSFStateFunctionExternal> hitReactionFunction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->whenHitReactionFunction = hitReactionFunction;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetHitReaction());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetHitReaction;
            t.hitReactionFunction = hitReactionFunction;
            return base.CopyTo(target);
        }
    }
}