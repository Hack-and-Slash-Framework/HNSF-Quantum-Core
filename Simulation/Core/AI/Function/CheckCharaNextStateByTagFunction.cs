namespace Quantum
{
    [System.Serializable]
    public unsafe class CheckCharaNextStateByTagFunction : AIFunction<bool>
    {
        public AssetRef<Tag> stateTag;
        
        public override bool Execute(FrameThreadSafe frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !csm->stateAgent.stateData.toStateRequested
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var stateRef)) return false;
            return csm->stateAgent.stateData.toState == stateRef;
        }
    }
}