using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateTypeRequirement : StatusEffectComponent
    {
        [System.Serializable]
        public class TagRequirements
        {
            [NonSerialized] public HashSet<AssetRef<Tag>> validTagsSet = new HashSet<AssetRef<Tag>>();
            [NonSerialized] public HashSet<AssetRef<Tag>> mustNotHaveTagsSet = new HashSet<AssetRef<Tag>>();
            
            public AssetRef<Tag>[] validTags = Array.Empty<AssetRef<Tag>>();
            public AssetRef<Tag>[] mustNotHaveTags = Array.Empty<AssetRef<Tag>>();
        }
        
        public TagRequirements applicationTagRequirements = new TagRequirements();
        public TagRequirements ongoingTagRequirements = new TagRequirements();
        public TagRequirements removalTagRequirements = new TagRequirements();

        public override void OnValidate(AssetObject statusEffectAsset)
        {
#if QUANTUM_UNITY
            BuildHashSets(applicationTagRequirements);
            BuildHashSets(ongoingTagRequirements);
            BuildHashSets(removalTagRequirements);
#endif
        }

        private void BuildHashSets(TagRequirements tagRequirements)
        {
            tagRequirements.validTagsSet = new HashSet<AssetRef<Tag>>(tagRequirements.validTags);
            tagRequirements.mustNotHaveTagsSet = new HashSet<AssetRef<Tag>>(tagRequirements.mustNotHaveTags);
        }

        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return ValidateStateRequirements(frame, statusEffector, applicationTagRequirements);
        }

        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return ValidateStateRequirements(frame, statusEffector, ongoingTagRequirements);
        }

        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return ValidateStateRequirements(frame, statusEffector, removalTagRequirements);
        }
        
        private bool ValidateStateRequirements(Frame frame, StatusEffector* statusEffector, TagRequirements requirements)
        {
            if(requirements.validTagsSet.Count == 0 && requirements.mustNotHaveTagsSet.Count == 0)
                return true;
            
            if(!frame.Unsafe.TryGetPointer<GenericStateMachine>(statusEffector->target, out var gsm))
                return false;
            if (!frame.TryFindAsset(gsm->stateAgent.stateData.state, out var currentStateAsset))
                return false;
            

            if (!requirements.validTagsSet.Contains(currentStateAsset.realSharedStateTag))
                return false;

            return !requirements.mustNotHaveTagsSet.Contains(currentStateAsset.realSharedStateTag);
        }
    }
}