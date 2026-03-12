using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Cutscene/Start Actor Cutscene")]
    public unsafe partial class StartActorCutscene : HNSFStateAction
    {
        [Serializable]
        public struct TagToTag
        {
            public AssetRef<Tag> entityTag;
            public bool dontControlPosition;
            public bool dontControlAnimation;
        }
        
        public bool autoPlay = true;
        public bool autoEnd = false;
        [DrawIf(nameof(autoEnd), true)] public int autoEndFrame;
        public AssetRef cutsceneSource;
        public AssetRef<Tag> cutsceneTag;

        public TagToTag[] cutsceneControlledEntities = Array.Empty<TagToTag>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            
            var sccEntity = frame.Create();
            var scc = new SyncedCutsceneSource()
            {
                sourcePlayer = entity,
                cutsceneSource = cutsceneSource,
                cutsceneTag = cutsceneTag,
                frame = 0,
                playrate = 1,
                autoPlay = autoPlay,
                autoEnd = autoEnd,
                endFrame = autoEndFrame
            };
            frame.Add(sccEntity, scc, out var sccResult);
            var d = frame.ResolveDictionary(sccResult->cutsceneControls);

            if (cutsceneControlledEntities == null || cutsceneControlledEntities.Length == 0) return false;
            
            foreach (var cce in cutsceneControlledEntities)
            {
                d[cce.entityTag] = new CutsceneEntityControlDefinition()
                {
                    controlPosition = !cce.dontControlPosition,
                    controlAnimation = !cce.dontControlAnimation,
                };
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new StartActorCutscene());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as StartActorCutscene;
            t.autoPlay = autoPlay;
            t.autoEnd = autoEnd;
            t.autoEndFrame = autoEndFrame;
            t.cutsceneSource = cutsceneSource;
            t.cutsceneTag = cutsceneTag;
            t.cutsceneControlledEntities = cutsceneControlledEntities.ToArray();
            return base.CopyTo(target);
        }
    }
}