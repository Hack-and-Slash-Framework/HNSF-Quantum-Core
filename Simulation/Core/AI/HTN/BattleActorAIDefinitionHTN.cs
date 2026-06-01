#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
using UnityEngine;
#endif

namespace Quantum
{
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "BattleActorAIDefinitionCWA")]
#endif
    public class BattleActorAIDefinitionHTN : BattleActorAIDefinition
    {
#if QUANTUM_UNITY
        [Header("Agent Info")]
#endif
        public AssetRef<HTNBehaviourDefinition> behaviourDefinition;
        public AssetRef<AIConfigBase> aiConfig;
        public AssetRef<AIBlackboardInitializer> blackboardInitRef;

        public override void Setup(Frame frame, EntityRef aiEntityRef, bool debug = false)
        {
            base.Setup(frame, aiEntityRef, debug);

            var agent = new HTNAgent()
            {
                behaviourDefinition = behaviourDefinition,
                cooldown = 0,
                currentActionData = default
            };
            frame.Add(aiEntityRef, agent);
        }
    }
}
