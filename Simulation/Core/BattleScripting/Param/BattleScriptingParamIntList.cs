using System.Collections.Generic;
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
    public unsafe sealed class BattleScriptingParamIntList : BattleScriptingParam<List<int>>
    {
        public static implicit operator BattleScriptingParamIntList(List<int> value)
        {
            return new BattleScriptingParamIntList() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override List<int> GetBlackboardValue(BlackboardValue value)
        {
            return new List<int>() { *value.IntegerValue };
        }

        protected override List<int> GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return new List<int>() { configPair.Value.Integer };
        }

        protected override List<int> GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override List<int> GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<List<int>>).Execute((Frame)frame, entity, ref context);
        }

        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }

        public override BattleScriptingParam<List<int>> Clone()
        {
            return new BattleScriptingParamIntList()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}