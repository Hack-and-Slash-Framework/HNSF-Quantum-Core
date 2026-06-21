using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Functions;
using HnSF.core.state;

namespace Quantum
{
    public enum GroupControlParamSource
    {
        None,
        Value,
        Config,
        Blackboard,
        Function,
    }
    
    [System.Serializable]
    public unsafe class BattleScriptingParam<T>
    {
        public HNSFParamSource Source = HNSFParamSource.Value;
        public string Key;
        public T DefaultValue;
        
        protected virtual T GetBlackboardValue(BlackboardValue value)
        {
            return default;
        }

        protected virtual T GetConfigValue(AIConfigBase.KeyValuePair configPair)
        {
            return default;
        }

        protected virtual T GetFunctionValue(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return default;
        }

        protected virtual T GetFunctionValue(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return default;
        }

        public virtual void SetFunction(GroupControlFunction newFunction)
        {
            
        }

        /// <summary>
        /// Use this to solve the AIParam value when the source of the value is unknown
        /// </summary>
        public T Resolve(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            if (Source == HNSFParamSource.Value || (Source != HNSFParamSource.Function && string.IsNullOrEmpty(Key) == true))
                return DefaultValue;

            switch (Source)
            {
                /*
                case HNSFParamSource.Blackboard:
                    BlackboardValue blackboardValue = context.blackboard->GetBlackboardValue(frame, Key);
                    return GetBlackboardValue(blackboardValue);
                case HNSFParamSource.Config:
                    AIConfigBase.KeyValuePair configPair = context.aiConfig?.Get(Key);
                    return configPair != null ? GetConfigValue(configPair) : DefaultValue;*/
                case HNSFParamSource.Function:
                    return GetFunctionValue(frame, entity, ref context);
            }
        
            return DefaultValue;
        }
    
        /// <summary>
        /// Use this if it is known that the AIParam stores specifically a Blackboard value
        /// </summary>
        public unsafe T ResolveBlackboard(Frame frame, AIBlackboardComponent* blackboard)
        {
            return ResolveBlackboard((FrameThreadSafe)frame, blackboard);
        }

        /// <summary>
        /// Use this if it is known that the AIParam stores specifically a Blackboard value
        /// </summary>
        public unsafe T ResolveBlackboard(FrameThreadSafe frame, AIBlackboardComponent* blackboard)
        {
            BlackboardValue blackboardValue = blackboard->GetBlackboardValue(frame, Key);
            return GetBlackboardValue(blackboardValue);
        }

    
        /// <summary>
        /// Use this if it is known that the HNSFParam stores specifically a Func
        /// </summary>
        public unsafe T ResolveFunction(Frame frame, EntityRef entity, ref BattleScriptContext context)
        {
            return ResolveFunction((FrameThreadSafe)frame, entity, ref context);
        }

        /// <summary>
        /// Use this if it is known that the HNSFParam stores specifically a Func
        /// </summary>
        public unsafe T ResolveFunction(FrameThreadSafe frame, EntityRef entity, ref BattleScriptContext context)
        {
            return GetFunctionValue(frame, entity, ref context);
        }

        public virtual BattleScriptingParam<T> Clone()
        {
            throw new NotImplementedException();
        }
    }
}