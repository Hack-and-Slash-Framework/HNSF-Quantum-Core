using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Functions;
using Quantum;
using Quantum.Collections;
#if QUANTUM_UNITY
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Functions
{
    [Serializable]
    public unsafe partial class GetParticipantActorEntityFiltered : GroupControlFunctionEntityRef
    {
        public enum TeamFilterType
        {
            None,
            Same,
            Different
        }
        
        public List<AssetRef<BattleActorDefinition>> battleActorFilter = new();
        public TeamFilterType teamFilter = TeamFilterType.None;
        public bool ignoreIfInTaggedEntityMap;
        
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            TaggedEntityMapping* tem = null;
            var hasTagMap = frame.Unsafe.TryGetPointer<TaggedEntityMapping>(infoEntityRef, out tem);
            QDictionary<AssetRef<Tag>, EntityRef> mappingDict = default;
            if(hasTagMap) mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            
            var selfParticipantLink = frame.Unsafe.GetPointer<ParticipantLink>(infoEntityRef);
            
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);

            var selfParticipantDataEntity = participantDataEntities[selfParticipantLink->id];
            
            foreach (var participantDataIntToEntity in participantDataEntities)
            {
                if(participantDataIntToEntity.Key == selfParticipantLink->id) continue;
                if(teamFilter != TeamFilterType.None && DoesNotMatchTeamFilter(frame, selfParticipantDataEntity, participantDataIntToEntity.Value)) continue;
                
                var participantBattleActorDefinitions = frame.Unsafe.GetPointer<ParticipantDataSelectedCharacters>(participantDataIntToEntity.Value);
                var participantBattleActorList = frame.ResolveList(participantBattleActorDefinitions->actorData);
                var participantBattleActorEntities = frame.Unsafe.GetPointer<ParticipantDataBattleActorEntities>(participantDataIntToEntity.Value);
                var participantActorEntityList = frame.ResolveList(participantBattleActorEntities->battleActorEntities);

                for (var index = 0; index < participantActorEntityList.Count; index++)
                {
                    if (!battleActorFilter.Contains(participantBattleActorList[index].battleActor)) continue;
                    if(ignoreIfInTaggedEntityMap && hasTagMap && EntityInMap(participantActorEntityList[index], mappingDict)) continue;
                    
                    return participantActorEntityList[index];
                }
            }
            return EntityRef.None;
        }
        
        private bool EntityInMap(EntityRef checkEntity, QDictionary<AssetRef<Tag>, EntityRef> mappings)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Value == checkEntity) return true;
            }
            return false;
        }
        
        private bool DoesNotMatchTeamFilter(Frame frame, EntityRef selfParticipantDataEntity, EntityRef otherParticipantDataEntity)
        {
            var aTeam = frame.Unsafe.GetPointer<CombatTeam>(selfParticipantDataEntity);
            var bTeam = frame.Unsafe.GetPointer<CombatTeam>(otherParticipantDataEntity);
            
            switch (teamFilter)
            {
                case TeamFilterType.Same:
                    return aTeam != bTeam;
                case TeamFilterType.Different:
                    return aTeam == bTeam;
                default:
                    return false;
            }
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class GetParticipantActorEntityFiltered : FunctionNodeBase
    {
        public const string OPTION_IGNORE = "IgnoreIfInMap";
        public const string inBattleActorFilter = "BattleActorFilter";
        public const string inTeamFilter = "TeamFilter";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<bool>(OPTION_IGNORE)
                .WithDisplayName("Ignore If In TaggedEntityMap?")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<List<AssetRef<BattleActorDefinition>>>(inBattleActorFilter)
                .WithDisplayName("Battle Actor Filter")
                .Build();
            
            context.AddInputPort<Functions.GetParticipantActorEntityFiltered.TeamFilterType>(inTeamFilter)
                .WithDisplayName("Team Filter")
                .WithDefaultValue(Functions.GetParticipantActorEntityFiltered.TeamFilterType.None)
                .Build();
        }

        public override GroupControlFunction Convert()
        {
            this.GetNodeOptionByName(OPTION_IGNORE).TryGetValue<bool>(out var ignore);
            
            return new Functions.GetParticipantActorEntityFiltered()
            {
                battleActorFilter = NodeHelper.GetInputPortValue<List<AssetRef<BattleActorDefinition>>>(GetInputPortByName(inBattleActorFilter)),
                teamFilter = NodeHelper.GetInputPortValue<Functions.GetParticipantActorEntityFiltered.TeamFilterType>(GetInputPortByName(inTeamFilter)),
                ignoreIfInTaggedEntityMap = ignore
            };
        }
    }
}
#endif