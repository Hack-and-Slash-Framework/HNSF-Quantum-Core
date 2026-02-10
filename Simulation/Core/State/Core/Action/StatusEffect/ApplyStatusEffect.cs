using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.StatusEffects;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ApplyStatusEffect : HNSFStateAction
    {
        public List<AssetRef<StatusEffectAsset>> statusEffectAssetRefs = new List<AssetRef<StatusEffectAsset>>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            frame.AddOrGet<StatusEffectActor>(targetEntityRef, out var statusEffectActor);

            for (int i = 0; i < statusEffectAssetRefs.Count; i++)
            {
                statusEffectActor->TryApplyStatusEffect(frame, targetEntityRef, statusEffectAssetRefs[i], ref stateContext);
            }
            
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyStatusEffect());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyStatusEffect;
            t.statusEffectAssetRefs = statusEffectAssetRefs.ToList();
            return base.CopyTo(target);
        }
    }
}