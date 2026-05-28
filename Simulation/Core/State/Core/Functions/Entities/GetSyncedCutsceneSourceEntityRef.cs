using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetSyncedCutsceneSourceEntityRef : StateFunctionEntityRef
    {
        public bool checkEntity;
        [DrawIf(nameof(checkEntity), true)]
        public StateActionTargetContext sourcePlayerContext;
        public bool checkCutsceneSource;
        [DrawIf(nameof(checkCutsceneSource), true)]
        public AssetRef cutsceneSourceTag;
        public bool checkCutsceneTag;
        [DrawIf(nameof(checkCutsceneTag), true)]
        public AssetRef<Tag> cutsceneTag;
        
        public override EntityRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var sourcePlayerEntityRef = EntityRef.None;
            if (checkEntity)
            {
                sourcePlayerContext.callingEntity = entity;
                sourcePlayerEntityRef = HNSFStateHelper.GetStateTargetEntity(frame, ref sourcePlayerContext);
            }

            var filter = frame.Filter<SyncedCutsceneSource>();
            while (filter.NextUnsafe(out var sourceEntityRef, out var scs))
            {
                if(checkEntity && scs->sourcePlayer != sourcePlayerEntityRef)
                    continue;
                if(checkCutsceneSource && scs->cutsceneSource != cutsceneSourceTag)
                    continue;
                if(checkCutsceneTag && scs->cutsceneTag != cutsceneTag)
                    continue;
                return sourceEntityRef;
            }
            return default;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetSyncedCutsceneSourceEntityRef());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetSyncedCutsceneSourceEntityRef;
            t.checkEntity = checkEntity;
            t.sourcePlayerContext = sourcePlayerContext;
            t.checkCutsceneSource = checkCutsceneSource;
            t.checkCutsceneTag = checkCutsceneTag;
            t.cutsceneSourceTag = cutsceneSourceTag;
            t.cutsceneTag = cutsceneTag;
            return base.CopyTo(target);
        }
    }
}