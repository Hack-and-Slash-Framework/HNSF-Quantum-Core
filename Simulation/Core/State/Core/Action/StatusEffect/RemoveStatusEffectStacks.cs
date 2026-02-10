using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.StatusEffects;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class RemoveStatusEffectStacks : HNSFStateAction
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
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<StatusEffectActor>(entity, out var statusEffectActor)) return false;

            var statusEffectList = frame.ResolveList(statusEffectActor->statusEffectors);
            if (statusEffectList.Count == 0) return false;

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
            
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new RemoveStatusEffectStacks());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as RemoveStatusEffectStacks;
            t.statusEffectsToRemove = statusEffectsToRemove.ToList();
            return base.CopyTo(target);
        }
    }
}