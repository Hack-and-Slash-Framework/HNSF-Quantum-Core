using HnSF.core.state;
using HnSF.core.state.functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class HNSFParamAssetRef : HNSFParam<AssetRef>
    {
        public static implicit operator HNSFParamAssetRef(AssetRef value)
        {
            return new HNSFParamAssetRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateFunction FunctionRef;

        protected override AssetRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.AssetRefValue;
        }

        protected override AssetRef GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.AssetRef;
        }

        protected override AssetRef GetFunctionValue(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref stateContext);
        }

        protected override AssetRef GetFunctionValue(FrameThreadSafe frame, EntityRef entity,
            ref HNSFStateContext stateContext)
        {
            return (FunctionRef as HNSFStateFunction<AssetRef>).Execute(frame, entity, ref stateContext);
        }

        public override HNSFParam<AssetRef> Clone()
        {
            return new HNSFParamAssetRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}