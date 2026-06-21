using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Functions
{
    [Serializable]
    public unsafe partial class GetParticipantActorEntity : GroupControlFunctionEntityRef
    {
        public BattleScriptingParamInt paramParticipantId;
        public BattleScriptingParamInt paramIndex;
        
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);

            var participantId = paramParticipantId.Resolve(frame, infoEntityRef, ref context);
            var index = paramIndex.Resolve(frame, infoEntityRef, ref context);
            
            if (!participantDataEntities.ContainsKey(participantId)) return default;
            var participantSpawnedActors = frame.Unsafe.GetPointer<ParticipantDataBattleActorEntities>(participantDataEntities[participantId]);
            var spawnedActorList = frame.ResolveList(participantSpawnedActors->battleActorEntities);
            if(spawnedActorList.Count <= index) return default;
            return spawnedActorList[index];
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class GetParticipantActorEntity : FunctionNodeBase
    {
        public const string PORT_PARTICIPANT_ID = "ParticipantId";
        public const string PORT_ACTOR_INDEX = "ActorIndex";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(PORT_PARTICIPANT_ID)
                .WithDisplayName("Participant Id Param")
                .Build();
            
            context.AddInputPort(PORT_ACTOR_INDEX)
                .WithDisplayName("Actor Index Param")
                .Build();
        }

        public override GroupControlFunction Convert()
        {
            return new Functions.GetParticipantActorEntity()
            {
                paramParticipantId = GetInputPortParam<BattleScriptingParamInt, int>(GetInputPortByName(PORT_PARTICIPANT_ID)),
                paramIndex = GetInputPortParam<BattleScriptingParamInt, int>(GetInputPortByName(PORT_ACTOR_INDEX)),
            };
        }
    }
}
#endif