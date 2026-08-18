using HnSF.core.AI.HTN.Functions;
using HnSF.core.GroupControl.Functions;
using HnSF.core.state;
using HnSF.core.state.functions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Param
{
    [System.Serializable]
    public unsafe sealed class HTNParamFPVector3 : HTNParam<FPVector3>
    {
        public static implicit operator HTNParamFPVector3(FPVector3 value)
        {
            return new HTNParamFPVector3() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override FPVector3 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector3Value;
        }

        protected override FPVector3 GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.FPVector3;
        }
        
        protected override FPVector3 GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<FPVector3>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<FPVector3> Clone()
        {
            return new HTNParamFPVector3()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}