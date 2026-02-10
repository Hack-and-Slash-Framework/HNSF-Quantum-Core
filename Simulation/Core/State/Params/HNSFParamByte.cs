
using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamByte : HNSFParam<byte>
    {
        public static implicit operator HNSFParamByte(byte value)
        {
            return new HNSFParamByte() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override byte GetBlackboardValue(BlackboardValue value)
        {
            return *value.ByteValue;
        }

        protected override byte GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.Byte;
        }

        protected override byte GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override byte GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<byte>).Execute(frame, entity, ref stateContext);
        }
        
        public override HNSFParam<byte> Clone()
        {
            return new HNSFParamByte()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}
