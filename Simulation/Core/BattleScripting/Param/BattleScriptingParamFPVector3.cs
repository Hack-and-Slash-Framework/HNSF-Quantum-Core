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
    public sealed unsafe class BattleScriptingParamFPVector3 : BattleScriptingParam<FPVector3>
    {
        public static implicit operator BattleScriptingParamFPVector3(FPVector3 value)
        {
            return new BattleScriptingParamFPVector3() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override FPVector3 GetBlackboardValue(BlackboardValue value)
        {
            return *value.FPVector3Value;
        }

        protected override FPVector3 GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.FPVector3;
        }

        protected override FPVector3 GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override FPVector3 GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<FPVector3>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<FPVector3> Clone()
        {
            return new BattleScriptingParamFPVector3()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}