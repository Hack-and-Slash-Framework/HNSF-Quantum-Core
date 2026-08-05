using System;
using HnSF.core.AI.HTN.Functions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Param
{
    [System.Serializable]
    public unsafe sealed class HTNParamString : HTNParam<string>
    {
        public static implicit operator HTNParamString(string value)
        {
            return new HTNParamString() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override string GetBlackboardValue(BlackboardValue value)
        {
            throw new NotSupportedException("Blackboard variables as strings are not supported.");
        }

        protected override string GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.String;
        }
        
        protected override string GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<string>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<string> Clone()
        {
            return new HTNParamString()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}