using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.state;
using Quantum;
using System.Collections.Generic;
#if QUANTUM_UNITY
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class SetParticipantBattleActor : GroupControlAction
    {
        public int participantId = 1;
        public List<AssetRef<BattleActorDefinition>> battleActorDefinitions;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);

            if (participantDataEntities.ContainsKey(participantId))
            {
                var participantDataEntity = participantDataEntities[participantId];
                var participantBattleActorDefinitions = frame.Unsafe.GetPointer<ParticipantDataSelectedCharacters>(participantDataEntity);
                var actorDataList = frame.ResolveList(participantBattleActorDefinitions->actorData);
                actorDataList.Clear();

                foreach (var bad in battleActorDefinitions)
                {
                    var actorData = new GamemodeParticipantBattleActorData
                    {
                        battleActor = bad,
                        specials = frame.AllocateList<AssetRef<HNSFSpecialSet>>()
                    };
                    actorDataList.Add(actorData);
                }
            }
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            return true;
        }
        
        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class SetParticipantBattleActor : ActorGroupControlNode
    {
        public const string IN_PORT_PARTICIPANT_ID = "ParticipantId";
        public const string IN_PORT_BATTLEACTOR = "BattleActor";
        public const string IN_PORT_BATTLEACTORS = "BattleActors";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<int>(IN_PORT_PARTICIPANT_ID)
                .WithDisplayName("Participant Id")
                .WithDefaultValue(1)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<List<AssetRef<BattleActorDefinition>>>(IN_PORT_BATTLEACTORS)
                .WithDisplayName("Battle Actors")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(IN_PORT_PARTICIPANT_ID).TryGetValue(out int participantId);
            
            return new Actions.SetParticipantBattleActor()
            {
                participantId = participantId,
                battleActorDefinitions = NodeHelper.GetInputPortValue<List<AssetRef<BattleActorDefinition>>>(GetInputPortByName(IN_PORT_BATTLEACTORS))
            };
        }
    }
}
#endif