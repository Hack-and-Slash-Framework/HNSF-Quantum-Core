using System;
using System.Collections.Generic;
using HnSF.core.state;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public partial class BattleActorDefinition : AssetObject
    {
        public AssetRef<EntityPrototype> CharacterPrototype;
        public AssetRef<AIConfig> parameters;
        public AssetRef<EntityView> overridedView;
        
        public AssetRef<Tag> defaultMoveset;
        public List<AssetRef<HNSFStateSet>> statesets = new List<AssetRef<HNSFStateSet>>();

#if QUANTUM_UNITY
        public ScriptableObject fighterDefinition;
        public GameObject cutsceneGroupingPrefab;
#endif
        
        public AssetRef<BattleActorAIDefinition>[] aiDefinitions = Array.Empty<AssetRef<BattleActorAIDefinition>>();

        public AssetRef<SystemsConfig> customSystems;
        public AssetRef<ExternalSystem>[] externalSystems = Array.Empty<AssetRef<ExternalSystem>>();
    }
}
