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
    public unsafe sealed class HTNParamByte : HTNParam<byte>
    {
        public static implicit operator HTNParamByte(byte value)
        {
            return new HTNParamByte() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override byte GetBlackboardValue(BlackboardValue value)
        {
            return *value.ByteValue;
        }

        protected override byte GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.Byte;
        }
        
        protected override byte GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<byte>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<byte> Clone()
        {
            return new HTNParamByte()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}