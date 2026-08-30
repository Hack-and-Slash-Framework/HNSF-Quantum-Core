namespace Quantum.HFSM.Functions
{
    [System.Serializable]
    public unsafe class CheckCharaNextStateByTag : AIFunction<bool>
    {
        public bool overrideMoveset = false;
        [DrawIf(nameof(overrideMoveset), true)]
        public AssetRef<Tag> toStateMovesetTag;
        public AssetRef<Tag> stateTag;
        
        public override bool Execute(FrameThreadSafe frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.TryGetPointer(entity, out BattleActorAI* battleActorAI)
                || !frame.TryGetPointer<GenericStateMachine>(battleActorAI->aiActorRef, out var csm)
                || !csm->stateAgent.stateData.toStateRequested
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !stateSet.AttemptGetStateByTag(overrideMoveset ? toStateMovesetTag : csm->stateAgent.stateData.moveset, stateTag, out var stateRef)) return false;
            return csm->stateAgent.stateData.toState == stateRef;
        }
    }
}