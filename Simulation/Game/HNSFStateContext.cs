using Quantum;

namespace HnSF.core.state
{
    public unsafe partial struct HNSFStateContext
    {
        public readonly HNSFStateAgentData* agentData;
        public readonly AIBlackboardComponent* blackboard;
        public readonly AIConfig aiConfig;
        public AssetRef<HNSFState> workingState;
        public int stateFrame;
        public int realStateFrame;
        public uint uniqueStateId;

        public HNSFStateContext(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out GenericStateMachine* stateMachine))
            {
                agentData = null;
                blackboard = null;
                aiConfig = null;
                workingState = default;
                stateFrame = 0;
                realStateFrame = 0;
                uniqueStateId = 0;
                return;
            }
            agentData = &stateMachine->stateAgent.stateData;
            blackboard = &stateMachine->blackboard;
            frame.TryFindAsset(stateMachine->config, out aiConfig);
            workingState = agentData->state;
            stateFrame = stateMachine->stateAgent.stateData.frame;
            realStateFrame = stateMachine->stateAgent.stateData.realFrame;
            uniqueStateId = stateMachine->stateAgent.stateData.uniqueStateId;
        }
        
        public HNSFStateContext(HNSFStateAgentData* agentData, AIBlackboardComponent* blackboard, AIConfig aiConfig, int stateFrame)
        {
            this.agentData = agentData;
            this.blackboard = blackboard;
            this.aiConfig = aiConfig;
            this.workingState = agentData->state;
            this.stateFrame = stateFrame;
            this.realStateFrame = agentData->realFrame;
            this.uniqueStateId = agentData->uniqueStateId;
        }
    
        public HNSFStateContext(HNSFStateAgentData* agentData, AIBlackboardComponent* blackboard, AIConfig aiConfig, AssetRef<HNSFState> workingState, int stateFrame)
        {
            this.agentData = agentData;
            this.blackboard = blackboard;
            this.aiConfig = aiConfig;
            this.workingState = workingState;
            this.stateFrame = stateFrame;
            this.realStateFrame = agentData->realFrame;
            this.uniqueStateId = agentData->uniqueStateId;
        }
    }
}