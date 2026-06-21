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
    public unsafe sealed class HTNParamFP : HTNParam<FP>
    {
        public static implicit operator HTNParamFP(FP value)
        {
            return new HTNParamFP() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override FP GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPValue;
        }

        protected override FP GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FP;
        }
        
        protected override FP GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<FP>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<FP> Clone()
        {
            return new HTNParamFP()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}