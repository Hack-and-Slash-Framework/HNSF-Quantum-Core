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
    public unsafe sealed class HTNParamFPVector2 : HTNParam<FPVector2>
    {
        public static implicit operator HTNParamFPVector2(FPVector2 value)
        {
            return new HTNParamFPVector2() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override FPVector2 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector2Value;
        }

        protected override FPVector2 GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.FPVector2;
        }
        
        protected override FPVector2 GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<FPVector2>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<FPVector2> Clone()
        {
            return new HTNParamFPVector2()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}