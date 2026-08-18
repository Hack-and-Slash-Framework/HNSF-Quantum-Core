using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Functions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe sealed class BattleScriptingParamString : BattleScriptingParam<string>
    {
        public static implicit operator BattleScriptingParamString(string value)
        {
            return new BattleScriptingParamString() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override string GetBlackboardValue(BlackboardValue value)
        {
            throw new NotSupportedException("Blackboard variables as strings are not supported.");
        }

        protected override string GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.String;
        }

        protected override string GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override string GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<string>).Execute((Frame)frame, entity, ref context);
        }

        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }

        public override BattleScriptingParam<string> Clone()
        {
            return new BattleScriptingParamString()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}