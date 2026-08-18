using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Functions;
using HnSF.core.state;
using HnSF.core.state.functions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class BattleScriptingParamAssetRef : BattleScriptingParam<AssetRef>
    {
        public static implicit operator BattleScriptingParamAssetRef(AssetRef value)
        {
            return new BattleScriptingParamAssetRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override AssetRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.AssetRefValue;
        }

        protected override AssetRef GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.AssetRef;
        }

        protected override AssetRef GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override AssetRef GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<AssetRef>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<AssetRef> Clone()
        {
            return new BattleScriptingParamAssetRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}