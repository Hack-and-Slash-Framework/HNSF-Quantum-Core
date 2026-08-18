
using HnSF.core.AI.HTN.Sensors;
using HnSF.core.AI.HTN.Tasks;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
using UnityEngine;
#endif

namespace Quantum
{
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "BattleActorAIDefinitionCWA")]
#endif
    public unsafe partial class BattleActorAIDefinitionHTN : BattleActorAIDefinition
    {
#if QUANTUM_UNITY
        [Header("Agent Info")]
#endif
        public AssetRef<DomainAssetObject> startingDomain;
        public AssetRef<AIConfig> aiConfig;
        public AssetRef<AIBlackboardInitializer> blackboardInitRef;
        public AssetRef<SensorGroup> sensorGroup;
        
        public override void Setup(Frame frame, EntityRef aiEntityRef, bool debug = false)
        {
            base.Setup(frame, aiEntityRef, debug);

            var agent = new HTNAgent()
            {
                domainAssetRef = startingDomain,
                sensorGroupAssetRef = sensorGroup,
                config = aiConfig,
                blackboardInitializer = blackboardInitRef,
                cooldown = 0,
                lastStatus = HTNTaskStatus.Uninitialized
            };
            frame.Add(aiEntityRef, agent);
        }
    }
}
