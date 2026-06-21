using System;
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
    public abstract unsafe class GroupControlParam<T>
    {
        public HNSFParamSource Source = HNSFParamSource.Value;
        public string Key;
        public T DefaultValue;
    
        protected abstract T GetBlackboardValue(BlackboardValue value);
        protected abstract T GetConfigValue(AIConfigBase.KeyValuePair configPair);
        protected abstract T GetFunctionValue(Frame frame, EntityRef entity);
        protected abstract T GetFunctionValue(FrameThreadSafe frame, EntityRef entity);
    
        /// <summary>
        /// Use this to solve the AIParam value when the source of the value is unknown
        /// </summary>
        public T Resolve(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (Source == HNSFParamSource.Value || (Source != HNSFParamSource.Function && string.IsNullOrEmpty(Key) == true))
                return DefaultValue;

            switch (Source)
            {
                case HNSFParamSource.Blackboard:
                    BlackboardValue blackboardValue = stateContext.blackboard->GetBlackboardValue(frame, Key);
                    return GetBlackboardValue(blackboardValue);
                case HNSFParamSource.Config:
                    AIConfigBase.KeyValuePair configPair = stateContext.aiConfig?.Get(Key);
                    return configPair != null ? GetConfigValue(configPair) : DefaultValue;
                case HNSFParamSource.Function:
                    return GetFunctionValue(frame, entity);
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
        public unsafe T ResolveFunction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return ResolveFunction((FrameThreadSafe)frame, entity);
        }

        /// <summary>
        /// Use this if it is known that the HNSFParam stores specifically a Func
        /// </summary>
        public unsafe T ResolveFunction(FrameThreadSafe frame, EntityRef entity)
        {
            return GetFunctionValue(frame, entity);
        }

        public virtual GroupControlParam<T> Clone()
        {
            throw new NotImplementedException();
        }
    }
}