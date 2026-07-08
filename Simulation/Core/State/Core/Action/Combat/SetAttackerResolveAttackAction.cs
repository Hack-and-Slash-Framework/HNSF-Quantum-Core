using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class SetAttackerResolveAttackAction : HNSFStateAction
    {
        public AssetRef<HitResolvePairAction> attackerResolveAction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->attackingResolveAction = attackerResolveAction;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAttackerResolveAttackAction());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAttackerResolveAttackAction;
            t.attackerResolveAction = attackerResolveAction;
            return base.CopyTo(target);
        }
    }
}