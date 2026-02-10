namespace Quantum
{
    [System.Serializable]
    public unsafe partial class HFSMCharaNextStateByTagDecision : HFSMDecision
    {
        public AssetRef<Tag> stateTag;
        
        public override bool Decide(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !csm->stateAgent.stateData.toStateRequested
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var stateRef)) return false;
            return csm->stateAgent.stateData.toState == stateRef;
        }
    }
}