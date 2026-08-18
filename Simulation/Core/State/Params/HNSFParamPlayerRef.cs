using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public sealed unsafe class HNSFParamPlayerRef : HNSFParam<PlayerRef>
    {
        public static implicit operator HNSFParamPlayerRef(PlayerRef value)
        {
            return new HNSFParamPlayerRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override PlayerRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.IntegerValue;
        }

        protected override PlayerRef GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.Integer;
        }

        protected override PlayerRef GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override PlayerRef GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<PlayerRef>).Execute(frame, entity, ref stateContext);
        }

        public override HNSFParam<PlayerRef> Clone()
        {
            return new HNSFParamPlayerRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}