
using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamInt : HNSFParam<int>
    {
        public static implicit operator HNSFParamInt(int value)
        {
            return new HNSFParamInt() { DefaultValue = value };
        }
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;
        
        protected override int GetBlackboardValue(BlackboardValue value)
        {
            return *value.IntegerValue;
        }

        protected override int GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.Integer;
        }

        protected override int GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override int GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<int>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<int> Clone()
        {
            return new HNSFParamInt()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}