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
    public unsafe sealed class BattleScriptingParamInt : BattleScriptingParam<int>
    {
        public static implicit operator BattleScriptingParamInt(int value)
        {
            return new BattleScriptingParamInt() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override int GetBlackboardValue(BlackboardValue value)
        {
            return *value.IntegerValue;
        }

        protected override int GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.Integer;
        }

        protected override int GetFunctionValue(Frame frame, EntityRef entity)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity);
        }

        protected override int GetFunctionValue(FrameThreadSafe frame, EntityRef entity)
        {
            return (FunctionRef as GroupControlFunction<int>).Execute((Frame)frame, entity);
        }

        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }

        public override BattleScriptingParam<int> Clone()
        {
            return new BattleScriptingParamInt()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}