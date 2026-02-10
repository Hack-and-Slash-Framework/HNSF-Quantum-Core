using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ApplyHitForceData : HNSFStateAction
    {
        public enum ForceType
        {
            None,
            LastHitByForceGrounded,
            LastHitByForceAerial,
            LastHitByGroundBounce,
            LastHitByWallBounce,
            Custom = 100
        }

        public ForceType forceType;
        [DrawIf(nameof(forceType), (int)ForceType.Custom)] public HitForceData customForceData;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitWithInfo>(entity, out var lastHitWithInfo)) return false;
            if (lastHitWithInfo->data.Field != Quantum.LastHitWithData.HITINFODATA
                || !frame.TryFindAsset<HitInfo>(lastHitWithInfo->data.hitInfoData->hitWithInfo.Id, out var hitWithInfo)) return false;
            
            switch (forceType)
            {
                case ForceType.LastHitByGroundBounce:
                    break;
                case ForceType.LastHitByWallBounce:
                    break;
                case ForceType.Custom:
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyHitForceData());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyHitForceData;
            t.forceType = forceType;
            t.customForceData = customForceData;
            return base.CopyTo(target);
        }
    }
}