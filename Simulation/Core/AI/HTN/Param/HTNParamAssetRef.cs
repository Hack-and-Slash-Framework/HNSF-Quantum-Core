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
    public unsafe sealed class HTNParamAssetRef : HTNParam<AssetRef>
    {
        public static implicit operator HTNParamAssetRef(AssetRef value)
        {
            return new HTNParamAssetRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunction FunctionRef;

        protected override AssetRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.AssetRefValue;
        }

        protected override AssetRef GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.AssetRef;
        }
        
        protected override AssetRef GetFunctionValue(ref HTNAgentContext context)
        {
            return (FunctionRef as HTNFunction<AssetRef>).Execute(ref context);
        }
        
        public override void SetFunction(HTNFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override HTNParam<AssetRef> Clone()
        {
            return new HTNParamAssetRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}