namespace Quantum
{
    public unsafe partial struct HNSFEventReceiver
    {
        public void InitializeEventType(Frame frame, int eventId)
        {
            var ag = frame.ResolveDictionary(actionGroups);
            
            if (!ag.ContainsKey(eventId))
            {
                var rGroup = new HNSFEventReceiverGroup();
                rGroup.actions = frame.AllocateDictionary<long, EventReceiverActions>();
                ag.Add(eventId, rGroup);
            }
        }

        public void UnregisterAllEventHandlesForKey(Frame frame, long key)
        {
            var ag = frame.ResolveDictionary(actionGroups);

            foreach (var a in ag)
            {
                a.Value.UnregisterActions(frame, key);
            }
        }
    }
}
