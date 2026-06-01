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
        public int participantId;
        public int index;
        
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef)
        {
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);

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
            
            context.AddInputPort<int>(PORT_PARTICIPANT_ID)
                .WithDisplayName("Participant Id")
                .WithDefaultValue(1)
                .Build();
            
            context.AddInputPort<int>(PORT_ACTOR_INDEX)
                .WithDisplayName("Actor Index")
                .WithDefaultValue(0)
                .Build();
        }

        public override GroupControlFunction Convert()
        {
            this.GetInputPortByName(PORT_PARTICIPANT_ID).TryGetValue(out int participantId);
            this.GetInputPortByName(PORT_ACTOR_INDEX).TryGetValue(out int actorIndex);
            
            return new HnSF.core.GroupControl.Functions.GetParticipantActorEntity()
            {
                participantId = participantId,
                index = actorIndex
            };
        }
    }
}
#endif