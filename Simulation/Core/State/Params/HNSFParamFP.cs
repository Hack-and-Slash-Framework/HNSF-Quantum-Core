using HnSF.core.state;
using HnSF.core.state.functions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamFP : HNSFParam<FP>
    {
        public static implicit operator HNSFParamFP(FP value)
        {
            return new HNSFParamFP() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override FP GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPValue;
        }

        protected override FP GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FP;
        }

        protected override FP GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override FP GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<FP>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<FP> Clone()
        {
            return new HNSFParamFP()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}