using System;
using System.Collections.Generic;
using HnSF.core.state;
using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateRequirement : StatusEffectComponent
    {
        [System.Serializable]
        public class TagRequirements
        {
            public HashSet<AssetRef<HNSFState>> validTagsSet = new();
            public HashSet<AssetRef<HNSFState>> mustNotHaveTagsSet = new();

            public AssetRef<HNSFState>[] validTags = Array.Empty<AssetRef<HNSFState>>();
            public AssetRef<HNSFState>[] mustNotHaveTags = Array.Empty<AssetRef<HNSFState>>();
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
            tagRequirements.validTagsSet = new HashSet<AssetRef<HNSFState>>(tagRequirements.validTags);
            tagRequirements.mustNotHaveTagsSet = new HashSet<AssetRef<HNSFState>>(tagRequirements.mustNotHaveTags);
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
            
            if(!frame.Unsafe.TryGetPointer<HNSFStateAgent>(statusEffector->target, out var hnsfStateAgent))
                return false;
            
            if (!requirements.validTagsSet.Contains(hnsfStateAgent->stateData.state))
                return false;

            return !requirements.mustNotHaveTagsSet.Contains(hnsfStateAgent->stateData.state);
        }
    }
}