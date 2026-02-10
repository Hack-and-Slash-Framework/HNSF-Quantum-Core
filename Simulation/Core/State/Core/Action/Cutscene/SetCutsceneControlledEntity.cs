using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Cutscene/Set Entity Controlled by Cutscene")]
    public unsafe partial class SetCutsceneControlledEntity : HNSFStateAction
    {
        [Serializable]
        public struct TagToTag
        {
            public AssetRef<Tag> entityTag;
            public bool dontControlPosition;
            public bool dontControlAnimation;
        }
        
        public AssetRef<Tag> cutsceneTag;
        public TagToTag[] cutsceneControlledEntities = Array.Empty<TagToTag>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var filtered = frame.Filter<SyncedCutsceneSource>();

            while (filtered.NextUnsafe(out var entityRef, out var scs))
            {
                if (scs->sourcePlayer != entity || scs->cutsceneTag != cutsceneTag) continue;
                var mapping = frame.ResolveDictionary(scs->cutsceneControls);

                foreach (var cce in cutsceneControlledEntities)
                {
                    mapping[cce.entityTag] = new CutsceneEntityControlDefinition()
                    {
                        controlAnimation = !cce.dontControlAnimation,
                        controlPosition = !cce.dontControlPosition
                    };
                }
                break;
            }
            
            frame.Events.UpdateCutsceneControlledEntities(entity, cutsceneTag);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetCutsceneControlledEntity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetCutsceneControlledEntity;
            t.cutsceneTag = cutsceneTag;
            t.cutsceneControlledEntities = cutsceneControlledEntities.ToArray();
            return base.CopyTo(target);
        }
    }
}