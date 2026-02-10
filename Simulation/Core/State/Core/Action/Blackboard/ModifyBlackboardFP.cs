using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Blackboard/Modify FP")]
    public unsafe partial class ModifyBlackboardFP : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            ADD,
            MULTIPLY
        }

        public ModifyType modifyType;
        public string key;
        public HNSFParamFP value = new HNSFParamFP();
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var originalValue = stateContext.blackboard->GetInteger(frame, key);
            var resolvedValue = value.Resolve(frame, entity, ref stateContext);

            switch (modifyType)
            {
                case ModifyType.SET:
                    stateContext.blackboard->Set(frame, key, resolvedValue);
                    break;
                case ModifyType.ADD:
                    stateContext.blackboard->Set(frame, key, originalValue + resolvedValue);
                    break;
                case ModifyType.MULTIPLY:
                    stateContext.blackboard->Set(frame, key, originalValue * resolvedValue);
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyBlackboardFP());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyBlackboardFP;
            t.modifyType = modifyType;
            t.key = key;
            t.value = value.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}