using Photon.Deterministic;

namespace Quantum.HFSM.Functions.Compound
{
    [System.Serializable]
    public unsafe class GetCompoundHFSMActionBlackboardFPVector3 : AIFunction<FPVector3>
    {
        public AIParamString blackboardRef;
        
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);

            if (!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return FPVector3.Zero;

            frame.TryFindAsset(aiContextUser->HFSMAgent->Config.Id, out AIConfig aiConfig);

            compoundAgent->ActionBb.TryGetVector3(frame, blackboardRef.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext), out var returnValue);
            return returnValue;
        }
    }
}