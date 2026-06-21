using HnSF.core.AI.HTN.Functions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Param
{
    [System.Serializable]
    public unsafe sealed class HTNParamBoolean : HTNParam<bool>
    {
        public static implicit operator HTNParamBoolean(bool value)
        {
            return new HTNParamBoolean() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override bool GetBlackboardValue(BlackboardValue value)
        {
            return *value.BooleanValue;
        }

        protected override bool GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.Boolean;
        }
        
        protected override bool GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<bool>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<bool> Clone()
        {
            return new HTNParamBoolean()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}