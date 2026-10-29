using System;
using System.Linq;
using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HasAnyGameTags : HNSFStateDecision
    {
        public GameTagCollection tags = new GameTagCollection();
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if(tags.IsEmpty)
                return true;
            
            var targetContext = new StateActionTargetContext(decisionTargetType, entity);
            var targetEntity = HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
            if (targetEntity == EntityRef.None)
                return false;

            if (!frame.Unsafe.TryGetPointer<GameTags>(targetEntity, out var gameTags))
                return false;
            
            return gameTags->HasAny(frame, tags);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasAnyGameTags());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HasAnyGameTags;
            t.tags = new GameTagCollection
            {
                Tags = tags.Tags.ToList()
            };
            return base.CopyTo(target);
        }
    }
}