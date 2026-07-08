using System;
using HnSF.core.state.functions;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class SetDefenderResolveAttackAction : HNSFStateAction
    {
        public AssetRef<HitResolvePairAction> defenderResolveAction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->defendingResolveAction = defenderResolveAction;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetDefenderResolveAttackAction());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetDefenderResolveAttackAction;
            t.defenderResolveAction = defenderResolveAction;
            return base.CopyTo(target);
        }
    }
}