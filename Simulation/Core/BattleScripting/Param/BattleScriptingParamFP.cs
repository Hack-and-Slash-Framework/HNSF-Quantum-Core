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
    public unsafe sealed class BattleScriptingParamFP : BattleScriptingParam<FP>
    {
        public static implicit operator BattleScriptingParamFP(FP value)
        {
            return new BattleScriptingParamFP() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override FP GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPValue;
        }

        protected override FP GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FP;
        }

        protected override FP GetFunctionValue(Frame frame, EntityRef entity)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity);
        }

        protected override FP GetFunctionValue(FrameThreadSafe frame, EntityRef entity)
        {
            return (FunctionRef as GroupControlFunction<FP>).Execute((Frame)frame, entity);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<FP> Clone()
        {
            return new BattleScriptingParamFP()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}