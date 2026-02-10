using Quantum;

namespace HnSF.core.systems
{
    public unsafe class HandleTickedEventReceivers : SystemMainThreadFilter<HandleTickedEventReceivers.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public TickedEventReceiver* tickedEventReceiver;
            public HNSFEventReceiver* hnsfEventReceiver;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            EventReceiverHelper.CallEvent(f, filter.Entity, (int)EventReceiverTyping.Tick);
            if (!EventReceiverHelper.HasEventsOfType(f, filter.Entity, (int)EventReceiverTyping.Tick))
            {
                f.Remove<TickedEventReceiver>(filter.Entity);
            }
        }
    }
}