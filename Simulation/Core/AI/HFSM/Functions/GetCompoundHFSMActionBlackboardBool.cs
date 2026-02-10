namespace Quantum.HFSM.Functions.Compound
{
    [System.Serializable]
    public unsafe class GetCompoundHFSMActionBlackboardBool: AIFunction<bool>
    {
        public AIParamString blackboardRef;
        
        public override bool Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);

            if (!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return false;

            frame.TryFindAsset(aiContextUser->HFSMAgent->Config.Id, out AIConfig aiConfig);

            compoundAgent->ActionBb.TryGetBoolean(frame, blackboardRef.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext), out QBoolean returnValue);
            return returnValue;
        }
    }
}