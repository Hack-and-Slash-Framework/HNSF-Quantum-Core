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
    public unsafe sealed class BattleScriptingParamFPVector2 : BattleScriptingParam<FPVector2>
    {
        public static implicit operator BattleScriptingParamFPVector2(FPVector2 value)
        {
            return new BattleScriptingParamFPVector2() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override FPVector2 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector2Value;
        }

        protected override FPVector2 GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FPVector2;
        }

        protected override FPVector2 GetFunctionValue(Frame frame, EntityRef entity)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity);
        }

        protected override FPVector2 GetFunctionValue(FrameThreadSafe frame, EntityRef entity)
        {
            return (FunctionRef as GroupControlFunction<FPVector2>).Execute((Frame)frame, entity);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<FPVector2> Clone()
        {
            return new BattleScriptingParamFPVector2()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}