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
    public sealed unsafe class BattleScriptingParamEntityRef : BattleScriptingParam<EntityRef>
    {
        public static implicit operator BattleScriptingParamEntityRef(EntityRef value)
        {
            return new BattleScriptingParamEntityRef() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override EntityRef GetBlackboardValue(BlackboardValue value)
        {
            return *value.EntityRefValue;
        }

        protected override EntityRef GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.EntityRef;
        }

        protected override EntityRef GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity, ref context);
        }

        protected override EntityRef GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return (FunctionRef as GroupControlFunction<EntityRef>).Execute((Frame)frame, entity, ref context);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<EntityRef> Clone()
        {
            return new BattleScriptingParamEntityRef()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}