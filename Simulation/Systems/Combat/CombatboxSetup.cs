using Quantum;

namespace HnSF.core.systems
{
    public unsafe class CombatboxSetup : SystemSignalsOnly, ISignalOnComponentAdded<BoxCombatant>, ISignalOnComponentRemoved<BoxCombatant>,
        ISignalOnComponentAdded<ArticlesOwner>, ISignalOnComponentRemoved<ArticlesOwner>
    {
        public void OnAdded(Frame f, EntityRef entity, BoxCombatant* component)
        {
            component->hurtboxList = f.AllocateList<EntityRef>();
            component->hitboxList = f.AllocateList<EntityRef>();
            component->collisionboxList = f.AllocateList<EntityRef>();
            component->warningboxList = f.AllocateList<EntityRef>();
            component->entitiesHit = f.AllocateList<EntityHitDefinition>();
            component->hitReactionCounters = f.AllocateDictionary<int, int>();
        }

        public void OnRemoved(Frame f, EntityRef entity, BoxCombatant* component)
        {
            BoxCombatantHelper.CleanupAllBoxes(f, component);
            
            f.FreeList(component->hurtboxList);
            f.FreeList(component->hitboxList);
            f.FreeList(component->collisionboxList);
            f.FreeList(component->warningboxList);
            f.FreeList(component->entitiesHit);
            f.FreeDictionary(component->hitReactionCounters);

            component->hurtboxList = default;
            component->hitboxList = default;
            component->collisionboxList = default;
            component->warningboxList = default;
            component->entitiesHit = default;
            component->hitReactionCounters = default;
        }
        
        public void OnAdded(Frame f, EntityRef entity, ArticlesOwner* component)
        {

            component->articleRefs = f.AllocateList<EntityRef>();
        }

        public void OnRemoved(Frame f, EntityRef entity, ArticlesOwner* component)
        {
            f.FreeList(component->articleRefs);
            component->articleRefs = default;
        }
    }
}
