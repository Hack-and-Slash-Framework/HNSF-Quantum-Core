
using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamBool : HNSFParam<bool>
    {
        public static implicit operator HNSFParamBool(bool value)
        {
            return new HNSFParamBool() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override bool GetBlackboardValue(BlackboardValue value)
        {
            return *value.BooleanValue;
        }

        protected override bool GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.Boolean;
        }

        protected override bool GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override bool GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<bool>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<bool> Clone()
        {
            return new HNSFParamBool()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}