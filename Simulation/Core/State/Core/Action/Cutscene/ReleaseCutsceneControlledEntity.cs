using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Cutscene/Release Entity From Cutscene")]
    public unsafe partial class ReleaseCutsceneControllledEntity : HNSFStateAction
    {
        public AssetRef<Tag> cutsceneTag;
        public AssetRef<Tag> entityTag;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            //frame.Events.UpdateCutsceneControlledEntities(entity, cutsceneTag);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ReleaseCutsceneControllledEntity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ReleaseCutsceneControllledEntity;
            t.cutsceneTag = cutsceneTag;
            t.entityTag = entityTag;
            return base.CopyTo(target);
        }
    }
}