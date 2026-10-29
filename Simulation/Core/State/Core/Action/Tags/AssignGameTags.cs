using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ApplyGameTags : HNSFStateAction
    {
        public GameTagCollection tagsToApply = new GameTagCollection();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            frame.AddOrGet<GameTags>(targetEntityRef, out var gameTags);

            gameTags->Set(frame, targetEntityRef, tagsToApply);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyGameTags());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyGameTags;
            t.tagsToApply = new GameTagCollection
            {
                Tags = tagsToApply.Tags.ToList()
            };
            return base.CopyTo(target);
        }
    }
}