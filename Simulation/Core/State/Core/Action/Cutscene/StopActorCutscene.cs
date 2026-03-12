using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Cutscene/Stop Actor Cutscene")]
    public unsafe partial class StopActorCutscene : HNSFStateAction
    {
        public AssetRef cutsceneSource;
        public AssetRef<Tag> cutsceneTag;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var syncedFilter = frame.Filter<SyncedCutsceneSource>();

            var entityToRemove = default(EntityRef);
            
            while (syncedFilter.NextUnsafe(out var syncedEntity, out var syncedCutsceneSource))
            {
                if (syncedCutsceneSource->sourcePlayer != entity
                    || syncedCutsceneSource->cutsceneTag != cutsceneTag
                    || syncedCutsceneSource->cutsceneSource != cutsceneSource) continue;

                entityToRemove = syncedEntity;
                break;
            }

            if (entityToRemove == default) return false;
            frame.Destroy(entityToRemove);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new StopActorCutscene());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as StopActorCutscene;
            t.cutsceneSource = cutsceneSource;
            t.cutsceneTag = cutsceneTag;
            return base.CopyTo(target);
        }
    }
}