using System;
using HnSF.core.AI.HTN.Functions;
using Quantum;

namespace HnSF.core.AI.HTN.Param
{
    [Serializable]
    public enum HTNParamSource
    {
        None,
        Value,
        Config,
        Blackboard,
        Function,
    }
    
    [System.Serializable]
    public unsafe class HTNParam<T>
    {
        public HTNParamSource Source = HTNParamSource.Value;
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

        protected virtual T GetFunctionValue(ref HTNAgentContext context)
        {
            return default;
        }
        
        public virtual void SetFunction(HTNFunction newFunction)
        {
            
        }

        /// <summary>
        /// Use this to solve the AIParam value when the source of the value is unknown
        /// </summary>
        public T Resolve(ref HTNAgentContext context)
        {
            if (Source == HTNParamSource.Value || (Source != HTNParamSource.Function && string.IsNullOrEmpty(Key) == true))
                return DefaultValue;

            switch (Source)
            {
                /*
                case HTNParamSource.Blackboard:
                    BlackboardValue blackboardValue = context.blackboard->GetBlackboardValue(frame, Key);
                    return GetBlackboardValue(blackboardValue);
                case HTNParamSource.Config:
                    AIConfigBase.KeyValuePair configPair = context.aiConfig?.Get(Key);
                    return configPair != null ? GetConfigValue(configPair) : DefaultValue;*/
                case HTNParamSource.Function:
                    return GetFunctionValue(ref context);
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
        public unsafe T ResolveFunction(ref HTNAgentContext context)
        {
            return GetFunctionValue(ref context);
        }

        public virtual HTNParam<T> Clone()
        {
            throw new NotImplementedException();
        }
    }
}