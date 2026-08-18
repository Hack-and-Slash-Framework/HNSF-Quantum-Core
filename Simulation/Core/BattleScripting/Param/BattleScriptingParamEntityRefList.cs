using System.Collections.Generic;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public sealed unsafe class BattleScriptingParamEntityRefList : BattleScriptingParam<List<EntityRef>>
    {
        public static implicit operator BattleScriptingParamEntityRefList(List<EntityRef> value)
        {
            return new BattleScriptingParamEntityRefList() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override List<EntityRef> GetBlackboardValue(BlackboardValue value)
        {
            return new List<EntityRef>() { *value.EntityRefValue };
        }

        protected override List<EntityRef> GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return new List<EntityRef>() { configPair.Value.EntityRef };
        }

        protected override List<EntityRef> GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override List<EntityRef> GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<List<EntityRef>>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<List<EntityRef>> Clone()
        {
            return new BattleScriptingParamEntityRefList()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}