using Quantum;

namespace HnSF.core.systems
{
    public unsafe class StatusEffectUpdate : SystemMainThreadFilter<StatusEffectUpdate.Filter>, ISignalOnComponentRemoved<StatusEffectActor>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public StatusEffectActor* StatusEffectActor;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var statusEffects = f.ResolveList(filter.StatusEffectActor->statusEffectors);

            for (int i = 0; i < statusEffects.Count; i++)
            {
                var statusEffectEntityRef = statusEffects[i];
                
                if(!f.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector)
                   || !f.TryFindAsset(statusEffector->statusEffetAssetRef, out var statusEffectAsset)) continue;
                
                statusEffectAsset.OnTick(f, statusEffectEntityRef);
            }
        }

        public void OnRemoved(Frame f, EntityRef entity, StatusEffectActor* component)
        {
            component->RemoveAllStatusEffects(f, entity);
            
            f.FreeList(ref component->statusEffectors);
            component->statusEffectors = default;
        }
    }
}