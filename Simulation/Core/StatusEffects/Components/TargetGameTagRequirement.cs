using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetGameTagRequirements : StatusEffectComponent
    {
        [System.Serializable]
        public class TagRequirements
        {
            public GameTagCollection mustHaveTags = new GameTagCollection();
            public GameTagCollection mustNotHaveTags = new GameTagCollection();
        }
        
        public TagRequirements applicationTagRequirements = new TagRequirements();
        public TagRequirements ongoingTagRequirements = new TagRequirements();
        public TagRequirements removalTagRequirements = new TagRequirements();

        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(applicationTagRequirements.mustHaveTags.IsEmpty && applicationTagRequirements.mustNotHaveTags.IsEmpty)
                return true;

            if (!frame.Unsafe.TryGetPointer<GameTags>(statusEffector->target, out var gameTags))
                return false;

            if (!gameTags->HasAll(frame, applicationTagRequirements.mustHaveTags))
                return false;
            return !gameTags->HasAny(frame, applicationTagRequirements.mustNotHaveTags);
        }
        
        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(ongoingTagRequirements.mustHaveTags.IsEmpty && ongoingTagRequirements.mustNotHaveTags.IsEmpty)
                return true;

            if (!frame.Unsafe.TryGetPointer<GameTags>(statusEffector->target, out var gameTags))
                return false;

            if (!gameTags->HasAll(frame, ongoingTagRequirements.mustHaveTags))
                return false;
            return !gameTags->HasAny(frame, ongoingTagRequirements.mustNotHaveTags);
        }
        
        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(removalTagRequirements.mustHaveTags.IsEmpty && removalTagRequirements.mustNotHaveTags.IsEmpty)
                return true;

            if (!frame.Unsafe.TryGetPointer<GameTags>(statusEffector->target, out var gameTags))
                return false;

            if (!gameTags->HasAll(frame, removalTagRequirements.mustHaveTags))
                return false;
            return !gameTags->HasAny(frame, removalTagRequirements.mustNotHaveTags);
        }
    }
}
