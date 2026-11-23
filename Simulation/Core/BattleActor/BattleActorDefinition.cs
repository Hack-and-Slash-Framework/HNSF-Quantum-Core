using System;
using System.Collections.Generic;
using HnSF;
using HnSF.core.GroupControl;
using HnSF.core.state;
using UnityEngine.Serialization;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public partial class BattleActorDefinition : AssetObject
    {
        public AssetRef<EntityPrototype> CharacterPrototype;
        [FormerlySerializedAs("parameters")] public AssetRef<AIConfig> config;
        public AssetRef<BattleActorParameters> actorParameters;
        public AssetRef<EntityView> overridedView;
        
        public AssetRef<Tag> defaultMoveset;
        public List<AssetRef<HNSFStateSet>> statesets = new List<AssetRef<HNSFStateSet>>();

#if QUANTUM_UNITY
        public ScriptableObject fighterDefinition;
        public GameObject cutsceneGroupingPrefab;
#endif
        
        public AssetRef<BattleActorGroupControlScript>[] introScripts = Array.Empty<AssetRef<BattleActorGroupControlScript>>();
        public AssetRef<ComboScriptListing>[] comboListings = Array.Empty<AssetRef<ComboScriptListing>>();
        public AssetRef<BattleActorAIDefinition>[] aiDefinitions = Array.Empty<AssetRef<BattleActorAIDefinition>>();

        [NonSerialized] public Dictionary<AssetRef<Tag>, ComboScriptListing> ComboListingByTag = new Dictionary<AssetRef<Tag>, ComboScriptListing>();
        [NonSerialized] public Dictionary<AssetRef<Tag>, BattleActorAIDefinition> aiDefinitionByTag = new Dictionary<AssetRef<Tag>, BattleActorAIDefinition>();
        
        public override void Loaded(IResourceManager resourceManager)
        {
            base.Loaded(resourceManager);

            ComboListingByTag.Clear();
            aiDefinitionByTag.Clear();
            
            if (comboListings != null)
            {
                foreach (var comboScriptListing in comboListings)
                {
                    if(!resourceManager.TryGetAsset(comboScriptListing, out var comboListing)
                       || comboListing.tag == default)
                        continue;
                    ComboListingByTag.Add(comboListing.tag, comboListing);
                }
            }

            if (aiDefinitions != null)
            {
                foreach (var aiDefinitionReference in aiDefinitions)
                {
                    if(!resourceManager.TryGetAsset(aiDefinitionReference, out var aiDefinition)
                       || aiDefinition.mainTag == default)
                        continue;
                    aiDefinitionByTag.Add(aiDefinition.mainTag, aiDefinition);
                }
            }
            
            LoadedUser(resourceManager);
        }

        partial void LoadedUser(IResourceManager resourceManager);
    }
}
