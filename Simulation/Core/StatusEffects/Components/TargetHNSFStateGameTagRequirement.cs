using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateGameTagRequirement : StatusEffectComponent
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
            return ValidateTagRequirements(frame, statusEffector, applicationTagRequirements);
        }

        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return ValidateTagRequirements(frame, statusEffector, ongoingTagRequirements);
        }

        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return ValidateTagRequirements(frame, statusEffector, removalTagRequirements);
        }
        
        private bool ValidateTagRequirements(Frame frame, StatusEffector* statusEffector, TagRequirements requirements)
        {
            if(requirements.mustHaveTags.IsEmpty && requirements.mustNotHaveTags.IsEmpty)
                return true;
            
            if(!frame.Unsafe.TryGetPointer<GenericStateMachine>(statusEffector->target, out var hnsfStateAgent))
                return false;
            if (!frame.TryFindAsset(hnsfStateAgent->stateAgent.stateData.state, out var currentStateAsset))
                return false;

            if (!currentStateAsset.gameTags.ContainsAll(frame, requirements.mustHaveTags))
                return false;

            return !currentStateAsset.gameTags.ContainsAny(frame, requirements.mustNotHaveTags);
        }
    }
}