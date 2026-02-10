using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CallEventHandlersForEntity : HNSFStateAction
    {
        public EventReceiverTyping eventType;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef);
            return false;
        }

        protected void DoAction(Frame frame, EntityRef entity)
        {
            EventReceiverHelper.CallEvent(frame, entity, (int)eventType);
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CallEventHandlersForEntity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            base.CopyTo(target);
            var t = target as CallEventHandlersForEntity;
            t.eventType = eventType;
            return target;
        }
    }
}