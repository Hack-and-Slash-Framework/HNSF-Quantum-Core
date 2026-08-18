namespace Quantum
{
    [System.Serializable]
    public unsafe class CheckStateMachineCurrentStateByTagFunction : AIFunction<bool>
    {
        public AssetRef<Tag> stateTag;
        
        public override bool Execute(FrameThreadSafe frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var stateRef))
            {
                Log.Error($"Can't find state from given tag. {stateTag}");
                return false;
            }
            return csm->stateAgent.stateData.state == stateRef;
        }
    }
}