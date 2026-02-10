using HnSF.core.state;
using HnSF.StatusEffects;

namespace Quantum
{
    public unsafe partial struct StatusEffectActor
    {
        public bool HasStatusEffect(Frame frame, AssetRef<StatusEffectAsset> statusEffectAssetRef)
        {
            var statusEffectEntityRefList = frame.ResolveList(statusEffectors);

            for (int i = 0; i < statusEffectEntityRefList.Count; i++)
            {
                var statusEffector = frame.Unsafe.GetPointer<StatusEffector>(statusEffectEntityRefList[i]);
                if (statusEffector->statusEffetAssetRef == statusEffectAssetRef) return true;
            }
            return false;
        }
        
        public bool TryApplyStatusEffect(Frame frame, EntityRef entity, AssetRef<StatusEffectAsset> statusEffectAssetRef, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(statusEffectAssetRef, out var statusEffectAsset)) return false;
            
            // Check Conditions
            bool conditionsValid = true;
            foreach (var deci in statusEffectAsset.applyConditions)
            {
                if(deci == null) continue;
                if (deci.Decide(frame, entity, ref stateContext)) return false;
                conditionsValid = false;
                break;
            }

            if (conditionsValid == false) return false;

            var statusEffectEntityRefList = frame.ResolveList(statusEffectors);

            // Check if status effect was already applied.
            // If so, see if we can add a stack.
            bool foundStatusEffect = false;
            for (int i = 0; i < statusEffectEntityRefList.Count; i++)
            {
                var statusEffector = frame.Unsafe.GetPointer<StatusEffector>(statusEffectEntityRefList[i]);
                if (statusEffector->statusEffetAssetRef != statusEffectAssetRef) continue;
                foundStatusEffect = true;

                if (statusEffectAsset.maxStacks > 0 && statusEffector->stacks >= statusEffectAsset.maxStacks) break;

                statusEffector->stacks += 1;
                statusEffectAsset.OnStackAdded(frame, statusEffectEntityRefList[i], 1);
            }

            if (foundStatusEffect) return true;

            var newStatusEffectEntityRef = frame.Create();
            frame.Add(newStatusEffectEntityRef, new StatusEffector()
            {
                actor = entity,
                stacks = 1,
                statusEffetAssetRef = statusEffectAssetRef
            });
            
            statusEffectEntityRefList.Add(newStatusEffectEntityRef);
            statusEffectAsset.OnApply(frame, newStatusEffectEntityRef);
            return true;
        }

        public void RemoveAllStatusEffects(Frame frame, EntityRef statusEffectActorEntityRef)
        {
            var statusEffectEntityRefList = frame.ResolveList(statusEffectors);
            
            for (int i = statusEffectEntityRefList.Count - 1; i >= 0; i--)
            {
                if (frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRefList[i], out var statusEffector)
                    &&frame.TryFindAsset<StatusEffectAsset>(statusEffector->statusEffetAssetRef, out var statusEffectAsset))
                {
                    statusEffectAsset.OnRemove(frame, statusEffectEntityRefList[i]);
                }

                frame.Destroy(statusEffectEntityRefList[i]);
                statusEffectEntityRefList.RemoveAt(i);
            }
            statusEffectEntityRefList.Clear();
        }
        
        public void RemoveStatusEffect(Frame frame, EntityRef statusEffectActorEntityRef, EntityRef statusEffectorEntityRef)
        {
            var statusEffectEntityRefList = frame.ResolveList(statusEffectors);

            for (int i = statusEffectEntityRefList.Count - 1; i >= 0; i--)
            {
                if(statusEffectEntityRefList[i] != statusEffectorEntityRef) continue;

                if (frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRefList[i], out var statusEffector)
                    &&frame.TryFindAsset<StatusEffectAsset>(statusEffector->statusEffetAssetRef, out var statusEffectAsset))
                {
                    statusEffectAsset.OnRemove(frame, statusEffectorEntityRef);
                }

                frame.Destroy(statusEffectorEntityRef);
                statusEffectEntityRefList.RemoveAt(i);
                break;
            }
        }

        public void RemoveStatusEffectsOfQualityType(Frame frame, EntityRef statusEffectActorEntityRef, StatusEffectQualityType qualityType)
        {
            var statusEffectEntityRefList = frame.ResolveList(statusEffectors);

            for (int i = statusEffectEntityRefList.Count - 1; i >= 0; i--)
            {
                if(!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRefList[i], out var statusEffector)
                   || !frame.TryFindAsset<StatusEffectAsset>(statusEffector->statusEffetAssetRef, out var statusEffectAsset)
                   || statusEffectAsset.qualityType != qualityType) continue;

                statusEffectAsset.OnRemove(frame, statusEffectEntityRefList[i]);
                
                frame.Destroy(statusEffectEntityRefList[i]);
                statusEffectEntityRefList.RemoveAt(i);
            }
        }
    }
}
