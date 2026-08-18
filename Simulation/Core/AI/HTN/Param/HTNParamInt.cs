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
    public unsafe sealed class HTNParamInt : HTNParam<int>
    {
        public static implicit operator HTNParamInt(int value)
        {
            return new HTNParamInt() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override int GetBlackboardValue(BlackboardValue value)
        {
            return *value.IntegerValue;
        }

        protected override int GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.Integer;
        }
        
        protected override int GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<int>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<int> Clone()
        {
            return new HTNParamInt()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}