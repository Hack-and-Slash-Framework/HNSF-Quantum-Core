using HnSF.core.state;
using HnSF.core.state.functions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamFPVector3 : HNSFParam<FPVector3>
    {
        public static implicit operator HNSFParamFPVector3(FPVector3 value)
        {
            return new HNSFParamFPVector3() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override FPVector3 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector3Value;
        }

        protected override FPVector3 GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.FPVector3;
        }

        protected override FPVector3 GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override FPVector3 GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<FPVector3>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<FPVector3> Clone()
        {
            return new HNSFParamFPVector3()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}