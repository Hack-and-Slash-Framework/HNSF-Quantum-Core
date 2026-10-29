using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class RemoveGameTags : HNSFStateAction
    {
        public GameTagCollection tagsToRemove = new GameTagCollection();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;

            if (!frame.Unsafe.TryGetPointer<GameTags>(targetEntityRef, out var gameTags))
                return false;

            gameTags->Clear(frame, targetEntityRef, tagsToRemove);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new RemoveGameTags());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as RemoveGameTags;
            t.tagsToRemove = new GameTagCollection
            {
                Tags = tagsToRemove?.Tags.ToList()
            };
            return base.CopyTo(target);
        }
    }
}