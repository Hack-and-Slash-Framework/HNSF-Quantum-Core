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
    }
}
