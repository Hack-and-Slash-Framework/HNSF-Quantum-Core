using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DirectDamage : HNSFStateAction
    {
        public AssetRef<HitInfo> hitInfo;
        public bool releaseThrowee;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DirectDamage());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DirectDamage;
            t.hitInfo = hitInfo;
            t.releaseThrowee = releaseThrowee;
            return base.CopyTo(target);
        }
    }
}