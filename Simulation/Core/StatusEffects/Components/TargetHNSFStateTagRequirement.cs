using System;
using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateTagRequirement : StatusEffectComponent
    {
        [System.Serializable]
        public class TagRequirements
        {
            public AssetRef<Tag>[] mustHaveTags = Array.Empty<AssetRef<Tag>>();
            public AssetRef<Tag>[] mustNotHaveTags = Array.Empty<AssetRef<Tag>>();
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
            if(requirements.mustHaveTags.Length == 0 && requirements.mustNotHaveTags.Length == 0)
                return true;
            
            if(!frame.Unsafe.TryGetPointer<GenericStateMachine>(statusEffector->target, out var hnsfStateAgent))
                return false;
            if (!frame.TryFindAsset(hnsfStateAgent->stateAgent.stateData.state, out var currentStateAsset))
                return false;

            if (!currentStateAsset.allTags.IsSupersetOf(requirements.mustHaveTags))
                return false;

            return !currentStateAsset.allTags.Overlaps(requirements.mustNotHaveTags);
        }
    }
}