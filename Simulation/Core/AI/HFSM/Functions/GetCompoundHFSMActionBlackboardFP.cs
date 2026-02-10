using Photon.Deterministic;

namespace Quantum.HFSM.Functions.Compound
{
    [System.Serializable]
    public unsafe class GetCompoundHFSMActionBlackboardFP : AIFunction<FP>
    {
        public AIParamString blackboardRef;
        
        public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);

            if (!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return 0;

            frame.TryFindAsset(aiContextUser->HFSMAgent->Config.Id, out AIConfig aiConfig);

            compoundAgent->ActionBb.TryGetInteger(frame, blackboardRef.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext), out var returnValue);
            return returnValue;
        }
    }
}