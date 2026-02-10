using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ChangeMoveset : HNSFStateAction
    {
        public AssetRef<Tag> moveset;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            HNSFStateHelper.ChangeMoveset(frame, targetEntityRef, moveset);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ChangeMoveset());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ChangeMoveset;
            t.moveset = moveset;
            return base.CopyTo(target);
        }
    }
}