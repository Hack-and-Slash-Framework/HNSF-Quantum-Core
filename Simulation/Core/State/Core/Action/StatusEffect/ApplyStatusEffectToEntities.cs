using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.state.functions;
using HnSF.StatusEffects;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ApplyStatusEffectToEntities : HNSFStateAction
    {
        public List<AssetRef<StatusEffectAsset>> statusEffectAssetRefs = new List<AssetRef<StatusEffectAsset>>();

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction getEntitiesFunction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var entityList = (getEntitiesFunction as HNSFStateFunction<List<EntityRef>>).Execute(frame, entity, ref stateContext);
            if (entityList == null || entityList.Count == 0) return false;

            for (int i = 0; i < entityList.Count; i++)
            {
                if(!frame.Exists(entityList[i])) continue;
                frame.AddOrGet<StatusEffectActor>(entityList[i], out var entityStatusEffectActor);
                
                for (int w = 0; w < statusEffectAssetRefs.Count; w++)
                {
                    entityStatusEffectActor->TryApplyStatusEffect(frame, entityList[i], statusEffectAssetRefs[w], ref stateContext);
                }
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyStatusEffectToEntities());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyStatusEffectToEntities;
            t.statusEffectAssetRefs = statusEffectAssetRefs.ToList();
            t.getEntitiesFunction = getEntitiesFunction;
            return base.CopyTo(target);
        }
    }
}