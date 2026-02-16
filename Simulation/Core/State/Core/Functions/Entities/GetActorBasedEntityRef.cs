using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetActorBasedEntityRef : StateFunctionEntityRef
    {
        public StateActionTargetContext targetContext;
        
        public override EntityRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            targetContext.callingEntity = entity;
            return HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetActorBasedEntityRef());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetActorBasedEntityRef;
            t.targetContext = targetContext;
            return base.CopyTo(target);
        }
    }
}