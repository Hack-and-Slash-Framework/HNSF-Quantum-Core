using System;

namespace Quantum
{
    [Serializable]
    public unsafe partial class ExecuteIfConditionAction : AIAction
    {
        public bool checkCondition;
        public AIParamBool condition;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var uData = ((AIContextUser*)aiContext.UserData);
            var aiConfig = frame.FindAsset(uData->HFSMAgent->Config);
            if (checkCondition && !condition.Resolve(frame, entity, uData->Blackboard, aiConfig, ref aiContext)) return;
            ConditionalExecute(frame, entity, ref aiContext, uData, aiConfig as AIConfig);
        }

        public virtual void ConditionalExecute(Frame frame, EntityRef entity, ref AIContext aiContext, AIContextUser* user, AIConfig aiConfig)
        {
            
        }
    }
}