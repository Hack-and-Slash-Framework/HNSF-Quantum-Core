using System.Collections.Generic;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public sealed unsafe class BattleScriptingParamAssetRefList : BattleScriptingParam<List<AssetRef>>
    {
        public static implicit operator BattleScriptingParamAssetRefList(List<AssetRef> value)
        {
            return new BattleScriptingParamAssetRefList() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override List<AssetRef> GetBlackboardValue(BlackboardValue value)
        {
            return new List<AssetRef>() { *value.AssetRefValue };
        }

        protected override List<AssetRef> GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return new List<AssetRef>() { configPair.Value.AssetRef };
        }

        protected override List<AssetRef> GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override List<AssetRef> GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<List<AssetRef>>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<List<AssetRef>> Clone()
        {
            return new BattleScriptingParamAssetRefList()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}