using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetTagRequirements : StatusEffectComponent
    {
        [System.Serializable]
        public class TagRequirements
        {
            public AssetRef<Tag>[] mustHaveTags;
            public AssetRef<Tag>[] mustNotHaveTags;
        }
        
        public TagRequirements applicationTagRequirements = new TagRequirements();
        public TagRequirements ongoingTagRequirements = new TagRequirements();
        public TagRequirements removalTagRequirements = new TagRequirements();

        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(applicationTagRequirements.mustHaveTags.Length == 0 && applicationTagRequirements.mustNotHaveTags.Length == 0)
                return true;
            
            if (!TagContainerHelper.HasAll(frame, statusEffector->target, applicationTagRequirements.mustHaveTags))
                return false;
            return !TagContainerHelper.HasAny(frame, statusEffector->target, applicationTagRequirements.mustNotHaveTags);
        }
        
        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(ongoingTagRequirements.mustHaveTags.Length == 0 && ongoingTagRequirements.mustNotHaveTags.Length == 0)
                return true;
            
            if (!TagContainerHelper.HasAll(frame, statusEffector->target, ongoingTagRequirements.mustHaveTags))
                return false;
            return !TagContainerHelper.HasAny(frame, statusEffector->target, ongoingTagRequirements.mustNotHaveTags);
        }
        
        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(removalTagRequirements.mustHaveTags.Length == 0 && removalTagRequirements.mustNotHaveTags.Length == 0)
                return true;
            
            if (!TagContainerHelper.HasAll(frame, statusEffector->target, removalTagRequirements.mustHaveTags))
                return false;
            return !TagContainerHelper.HasAny(frame, statusEffector->target, removalTagRequirements.mustNotHaveTags);
        }
    }
}
