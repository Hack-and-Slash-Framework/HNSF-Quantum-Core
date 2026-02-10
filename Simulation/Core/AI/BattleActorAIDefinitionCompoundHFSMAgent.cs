#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe partial class BattleActorAIDefinitionCompoundHFSMAgent : BattleActorAIDefinition
    {
#if QUANTUM_UNITY
        [Header("Agent Info")]
#endif
        public AssetRef<AIConfig> aiConfig;
        
        public AssetRef<HFSMRoot> initialRootBrain;
        public AssetRef<HFSMRoot> initialActionBrain;
        
        public AssetRef<AIBlackboardInitializer> brainBlackboardInitRef;
        public AssetRef<AIBlackboardInitializer> actionBlackboardInitRef;
        
        public override void Setup(Frame frame, EntityRef aiEntityRef, bool debug = false)
        {
            base.Setup(frame, aiEntityRef, debug);

            var compoundAgent = new HFSMCompoundAgent()
            {
                Brain = new HFSMAgent(),
                Action = new HFSMAgent(),
            };

            compoundAgent.Brain.Config = aiConfig.Id;
            compoundAgent.Brain.Data.Root = initialRootBrain;

            compoundAgent.Action.Config = aiConfig.Id;
            compoundAgent.Action.Data.Root = initialActionBrain;
            
            if (frame.TryFindAsset<AIBlackboardInitializer>(brainBlackboardInitRef, out var brainInit)) 
                AIBlackboardInitializer.InitializeBlackboard(frame, &compoundAgent.BrainBb, brainInit);
            if (frame.TryFindAsset<AIBlackboardInitializer>(actionBlackboardInitRef, out var actionInit)) 
                AIBlackboardInitializer.InitializeBlackboard(frame, &compoundAgent.ActionBb, actionInit);
            
            frame.Add(aiEntityRef, compoundAgent);
        }
    }
}