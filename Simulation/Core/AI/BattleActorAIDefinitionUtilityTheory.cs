#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe partial class BattleActorAIDefinitionUtilityTheory : BattleActorAIDefinition
    {
#if QUANTUM_UNITY
        [Header("Agent Info")]
#endif
        public AssetRef<AIConfigBase> aiConfig;
        public AssetRef<UTRoot> root;
        public AssetRef<AIBlackboardInitializer> blackboardInitRef;
        
        public override void Setup(Frame frame, EntityRef aiEntityRef, bool debug = false)
        {
            base.Setup(frame, aiEntityRef, debug);
            
            if (frame.TryFindAsset(blackboardInitRef, out var blackboardInitAsset))
            {
                var blackboardComponent = new AIBlackboardComponent();
                AIBlackboardInitializer.InitializeBlackboard(frame, &blackboardComponent, blackboardInitAsset);
                frame.Set(aiEntityRef, blackboardComponent);
            }
            
            var agent = new UTAgent()
            {
                Config = aiConfig,
                UtilityReasoner = new UtilityReasoner()
                {
                    UTRoot = root
                }
            };
            frame.Add(aiEntityRef, agent);
        }
    }
}