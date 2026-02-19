using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public class TargetTagRequirements : StatusEffectComponent
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
    }
}
