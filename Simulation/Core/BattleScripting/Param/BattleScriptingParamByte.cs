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
    public unsafe sealed class BattleScriptingParamByte : BattleScriptingParam<byte>
    {
        public static implicit operator BattleScriptingParamByte(byte value)
        {
            return new BattleScriptingParamByte() { DefaultValue = value };
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunction FunctionRef;

        protected override byte GetBlackboardValue(BlackboardValue value)
        {
            return *value.ByteValue;
        }

        protected override byte GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return configPair.Value.Byte;
        }

        protected override byte GetFunctionValue(Frame frame, EntityRef entity)
        {
            return GetFunctionValue((FrameThreadSafe)frame, entity);
        }

        protected override byte GetFunctionValue(FrameThreadSafe frame, EntityRef entity)
        {
            return (FunctionRef as GroupControlFunction<byte>).Execute((Frame)frame, entity);
        }
        
        public override void SetFunction(GroupControlFunction newFunction)
        {
            FunctionRef = newFunction;
        }
        
        public override BattleScriptingParam<byte> Clone()
        {
            return new BattleScriptingParamByte()
            {
                Source = Source,
                Key = Key,
                DefaultValue = DefaultValue,
                FunctionRef = FunctionRef?.Copy()
            };
        }
    }
}