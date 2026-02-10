using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Blackboard/Modify Boolean")]
    public unsafe partial class ModifyBlackboardBool : HNSFStateAction
    {
        public enum ModifyType
        {
            Set,
            Inverse
        }

        public ModifyType modifyType;
        public string key;
        public HNSFParamBool value = new HNSFParamBool();
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var originalValue = stateContext.blackboard->GetBoolean(frame, key);

            switch (modifyType)
            {
                case ModifyType.Set:
                    stateContext.blackboard->Set(frame, key, value.Resolve(frame, entity, ref stateContext));
                    break;
                case ModifyType.Inverse:
                    stateContext.blackboard->Set(frame, key, !originalValue);
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyBlackboardBool());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyBlackboardBool;
            t.modifyType = modifyType;
            t.key = key;
            t.value = value.Clone() as HNSFParamBool;
            return base.CopyTo(target);
        }
    }
}