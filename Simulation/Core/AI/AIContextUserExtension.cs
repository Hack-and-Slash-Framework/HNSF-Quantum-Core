namespace Quantum
{
    public unsafe partial struct AIContextUser
    {
        public readonly AIBlackboardComponent* Blackboard;
        public readonly HFSMAgent* HFSMAgent;

        public readonly AIContextUserType DataType;
        public readonly unsafe void* DataPointer;
        
        public AIContextUser(AIBlackboardComponent* blackboard)
        {
            Blackboard = blackboard;
            HFSMAgent = null;
            DataType = AIContextUserType.None;
            DataPointer = null;
        }
        
        public AIContextUser(AIBlackboardComponent* blackboard, HFSMAgent* hfsmAgent)
        {
            Blackboard = blackboard;
            HFSMAgent = hfsmAgent;
            DataType = AIContextUserType.None;
            DataPointer = null;
        }
        
        public AIContextUser(AIBlackboardComponent* blackboard, HFSMAgent* hfsmAgent, AIContextUserType dataType, void* data)
        {
            Blackboard = blackboard;
            HFSMAgent = hfsmAgent;
            DataType = dataType;
            DataPointer = data;
        }
    }
}