using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Blackboard/Modify Int")]
    public unsafe partial class ModifyBlackboardInt : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            ADD,
            MULTIPLY
        }

        public ModifyType modifyType;
        public string key;
        public HNSFParamInt value;
        public bool clampValue;
        public HNSFParamInt minValue;
        public HNSFParamInt maxValue;
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var finalValue = stateContext.blackboard->GetInteger(frame, key);
            var resolvedValue = value.Resolve(frame, entity, ref stateContext);

            switch (modifyType)
            {
                case ModifyType.SET:
                    finalValue = resolvedValue;
                    break;
                case ModifyType.ADD:
                    finalValue += resolvedValue;
                    break;
                case ModifyType.MULTIPLY:
                    finalValue *= resolvedValue;
                    break;
            }
            
            if (clampValue)
            {
                var minV = minValue.Resolve(frame, entity, ref stateContext);
                var maxV = maxValue.Resolve(frame, entity, ref stateContext);

                finalValue = Math.Clamp(finalValue, minV, maxV);
            }
            
            stateContext.blackboard->Set(frame, key, finalValue);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyBlackboardInt());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyBlackboardInt;
            t.modifyType = modifyType;
            t.key = key;
            t.value = value.Clone() as HNSFParamInt;
            t.clampValue = clampValue;
            t.minValue = minValue.Clone() as HNSFParamInt;
            t.maxValue = maxValue.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}