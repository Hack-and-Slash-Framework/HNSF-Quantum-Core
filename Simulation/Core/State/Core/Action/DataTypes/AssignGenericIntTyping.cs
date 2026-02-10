using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class AssignGenericIntTyping : HNSFStateAction
    {
        public StateActionTargetContext assignedTargetContext;
        public HNSFParamInt assignedValueParam;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            assignedTargetContext.callingEntity = targetEntityRef;
            var assignedTargetEntityRef = HNSFStateHelper.GetStateTargetEntity(frame, ref assignedTargetContext);
            if (assignedTargetEntityRef == EntityRef.None) return false;
            
            var value = assignedValueParam.Resolve(frame, targetEntityRef, ref stateContext);
            frame.AddOrGet<AssignedIntTypingGeneric>(assignedTargetEntityRef, out var aitg);
            aitg->typing = value;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new AssignGenericIntTyping());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as AssignGenericIntTyping;
            t.assignedValueParam = assignedValueParam.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}