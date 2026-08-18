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
    public sealed unsafe class BattleScriptingParamFPVector3List : BattleScriptingParam<List<FPVector3>>
    {
        public static implicit operator BattleScriptingParamFPVector3List(List<FPVector3> value)
        {
            return new BattleScriptingParamFPVector3List() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override List<FPVector3> GetBlackboardValue(BlackboardValue value)
        {
            return new List<FPVector3>(){ *value.FPVector3Value };
        }

        protected override List<FPVector3> GetConfigValue(AIConfig.KeyValuePair configPair)
        {
            return new List<FPVector3>(){ configPair.Value.FPVector3 };
        }

        protected override List<FPVector3> GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override List<FPVector3> GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<List<FPVector3>>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<List<FPVector3>> Clone()
        {
            return new BattleScriptingParamFPVector3List()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}