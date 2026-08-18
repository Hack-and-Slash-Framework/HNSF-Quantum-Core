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
    public unsafe sealed class BattleScriptingParamBoolean : BattleScriptingParam<bool>
    {
        public static implicit operator BattleScriptingParamBoolean(bool value)
        {
            return new BattleScriptingParamBoolean() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override bool GetBlackboardValue(BlackboardValue value)
        {
            return *value.BooleanValue;
        }

        protected override bool GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return configPair.Value.Boolean;
        }

        protected override bool GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override bool GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<bool>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<bool> Clone()
        {
            return new BattleScriptingParamBoolean()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}