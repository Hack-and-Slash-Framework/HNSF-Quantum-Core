namespace Quantum
{
    public static unsafe partial class EventReceiverHelper
    {
        public static void CallEvent(Frame frame, int eventId)
        {
            var filter = frame.Filter<HNSFEventReceiver>();

            while (filter.NextUnsafe(out var entityRef, out var eventReceiver))
            {
                var agDictionary = frame.ResolveDictionary(eventReceiver->actionGroups);
                if(!agDictionary.ContainsKey(eventId)) continue;

                var actions = frame.ResolveDictionary(agDictionary[eventId].actions);

                foreach (var ac in actions)
                {
                    var ls = frame.ResolveList(ac.Value.actions);

                    foreach (var a in ls)
                    {
                        if (!frame.TryFindAsset(a, out var actionAsset)) continue;
                        actionAsset.Execute(frame, entityRef);
                    }
                }
            }
        }
        
        public static void CallEvent(Frame frame, EntityRef entityRef, int eventId)
        {
            if (!frame.Unsafe.TryGetPointer<HNSFEventReceiver>(entityRef, out var eventReceiver)) return;
            
            var agDictionary = frame.ResolveDictionary(eventReceiver->actionGroups);
            if (!agDictionary.ContainsKey(eventId)) return;

            var actions = frame.ResolveDictionary(agDictionary[eventId].actions);

            foreach (var ac in actions)
            {
                var ls = frame.ResolveList(ac.Value.actions);

                foreach (var a in ls)
                {
                    if (!frame.TryFindAsset(a, out var actionAsset)) continue;
                    actionAsset.Execute(frame, entityRef);
                }
            }
        }

        public static void Unregister(Frame frame, EntityRef entityRef, long actionGroupId)
        {
            if (!frame.Unsafe.TryGetPointer<HNSFEventReceiver>(entityRef, out var eventReceiver)) return;
            eventReceiver->UnregisterAllEventHandlesForKey(frame, actionGroupId);
        }
        
        public static bool HasEventsOfType(Frame frame, EntityRef entityRef, int eventId)
        {
            if (!frame.Unsafe.TryGetPointer<HNSFEventReceiver>(entityRef, out var eventReceiver)) return false;
            
            var agDictionary = frame.ResolveDictionary(eventReceiver->actionGroups);
            if (!agDictionary.ContainsKey(eventId)) return false;
            
            var actions = frame.ResolveDictionary(agDictionary[eventId].actions);
            if (actions.Count == 0) return false;
            return true;
        }
    }
}
