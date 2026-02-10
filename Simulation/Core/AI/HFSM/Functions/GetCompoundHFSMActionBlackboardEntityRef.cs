namespace Quantum.HFSM.Functions.Compound
{
    [System.Serializable]
    public unsafe class GetCompoundHFSMActionBlackboardEntityRef : AIFunction<EntityRef>
    {
        public AIParamString blackboardRef;
        
        public override EntityRef Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);
            
            if(!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return EntityRef.None;

            frame.TryFindAsset(aiContextUser->HFSMAgent->Config.Id, out AIConfig aiConfig);

            compoundAgent->ActionBb.TryGetEntityRef(frame, blackboardRef.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext), out var returnValue);
            return returnValue;
        }
    }
}