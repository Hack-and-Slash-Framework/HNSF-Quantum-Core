using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public sealed unsafe class HNSFParamEntityRef : HNSFParam<EntityRef>
    {
        public static implicit operator HNSFParamEntityRef(EntityRef value)
        {
            return new HNSFParamEntityRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override EntityRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.EntityRefValue;
        }

        protected override EntityRef GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.EntityRef;
        }

        protected override EntityRef GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override EntityRef GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<EntityRef>).Execute(frame, entity, ref stateContext);
        }

        public override HNSFParam<EntityRef> Clone()
        {
            return new HNSFParamEntityRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}