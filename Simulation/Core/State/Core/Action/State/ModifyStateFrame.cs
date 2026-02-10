using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyStateFrame : HNSFStateAction
    {
        public enum ModifyType
        {
            Set,
            Add
        }

        public ModifyType modifyType;
        public int value;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            
            switch (modifyType)
            {
                case ModifyType.Set:
                    targetStateContext.agentData->frame = value;
                    break;
                case ModifyType.Add:
                    targetStateContext.agentData->frame += value;
                    break;
            }
            targetStateContext.agentData->dontAutoIncrementFrame = true;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyStateFrame());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyStateFrame;
            t.modifyType = modifyType;
            t.value = value;
            return base.CopyTo(target);
        }
    }
}