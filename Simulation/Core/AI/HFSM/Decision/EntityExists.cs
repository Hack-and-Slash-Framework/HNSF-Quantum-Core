namespace Quantum.HFSM.Decisions
{
    [System.Serializable]
    public unsafe partial class EntityExists : HFSMDecision
    {
        public AIParamEntityRef entityRefParam;
        public bool invert;
        
        public override bool Decide(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);

            AIConfigBase aiConfig = frame.FindAsset(aiContextUser->HFSMAgent->Config);
            var entityRef = entityRefParam.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext);
            
            var result = frame.Exists(entityRef);
            if(invert) result = !result;
            return result;
        }
    }
}