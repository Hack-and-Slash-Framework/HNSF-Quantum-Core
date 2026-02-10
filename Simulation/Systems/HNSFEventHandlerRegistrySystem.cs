using Quantum;

namespace HnSF.core.systems
{
    public class EventHandlerRegistrySystem : SystemSignalsOnly, ISignalOnComponentAdded<HNSFEventReceiver>, ISignalOnComponentRemoved<HNSFEventReceiver>
    {
        public unsafe void OnAdded(Frame f, EntityRef entity, HNSFEventReceiver* component)
        {
            component->actionGroups = f.AllocateDictionary<int, HNSFEventReceiverGroup>();
        }

        public unsafe void OnRemoved(Frame f, EntityRef entity, HNSFEventReceiver* component)
        {
            var actionGroups = f.ResolveDictionary(component->actionGroups);

            foreach (var actionGroup in actionGroups)
            {
                var actions = f.ResolveDictionary(actionGroup.Value.actions);

                foreach (var actionList in actions)
                {
                   f.FreeList(actionList.Value.actions);
                }
                actions.Clear();
                
                f.FreeDictionary<long, EventReceiverActions>(actions);
            }
            
            actionGroups.Clear();
            f.FreeDictionary<int, HNSFEventReceiverGroup>(component->actionGroups);
        }
    }
}