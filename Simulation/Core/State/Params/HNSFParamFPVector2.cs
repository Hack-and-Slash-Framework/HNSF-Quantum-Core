using HnSF.core.state;
using HnSF.core.state.functions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamFPVector2 : HNSFParam<FPVector2>
    {
        public static implicit operator HNSFParamFPVector2(FPVector2 value)
        {
            return new HNSFParamFPVector2() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override FPVector2 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector2Value;
        }

        protected override FPVector2 GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FPVector2;
        }

        protected override FPVector2 GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override FPVector2 GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<FPVector2>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<FPVector2> Clone()
        {
            return new HNSFParamFPVector2()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}