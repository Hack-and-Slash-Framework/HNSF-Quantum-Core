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
    public unsafe partial class RemoveStatusEffectStacksFromEntities : HNSFStateAction
    {
        [Serializable]
        public struct StatusEffectStackRemovalInfo
        {
            public AssetRef<StatusEffectAsset> statusEffectAssetRef;
            public bool removeAllStacks;
            [DrawIf(nameof(removeAllStacks), false)]
            public int stacksToRemove;
        }

        public List<StatusEffectStackRemovalInfo> statusEffectsToRemove = new List<StatusEffectStackRemovalInfo>();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction getEntitiesFunction;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var entityList = (getEntitiesFunction as HNSFStateFunction<List<EntityRef>>).Execute(frame, entity, ref stateContext);
            if (entityList == null || entityList.Count == 0) return false;

            foreach (var entityRef in entityList)
            {
                if (!frame.Unsafe.TryGetPointer<StatusEffectActor>(entityRef, out var statusEffectActor)) continue;

                var statusEffectList = frame.ResolveList(statusEffectActor->statusEffectors);
                if (statusEffectList.Count == 0) continue;

                for (int i = statusEffectList.Count - 1; i >= 0; i--)
                {
                    if(!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectList[i], out var statusEffector)
                       || statusEffector->stacks == 0) continue;
                
                    for (int j = 0; j < statusEffectsToRemove.Count; j++)
                    {
                        if(statusEffector->statusEffetAssetRef != statusEffectsToRemove[i].statusEffectAssetRef) continue;
                        statusEffector->RemoveStacks(statusEffectsToRemove[i].stacksToRemove, statusEffectsToRemove[i].removeAllStacks);
                        break;
                    }
                }
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new RemoveStatusEffectStacksFromEntities());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as RemoveStatusEffectStacksFromEntities;
            t.statusEffectsToRemove = statusEffectsToRemove.ToList();
            t.getEntitiesFunction = getEntitiesFunction;
            return base.CopyTo(target);
        }
    }
}