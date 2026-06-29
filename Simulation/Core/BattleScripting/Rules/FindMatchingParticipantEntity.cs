using System;
using System.Collections.Generic;
using Quantum;
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    public unsafe partial class FindMatchingParticipantEntity : GroupControlRule
    {
        public enum TeamFilterType
        {
            None,
            Same,
            Different
        }

        public AssetRef<Tag> assignedTag;
        public List<AssetRef<BattleActorDefinition>> battleActorFilter = new();
        public TeamFilterType teamFilter = TeamFilterType.None;
        public bool ignoreIfInTaggedEntityMap;
        public bool clearMappingFirst = true;
        
        public override bool IsValid(Frame frame, EntityRef infoEntityRef)
        {
            frame.AddOrGet<TaggedEntityMapping>(infoEntityRef, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);

            if (clearMappingFirst) mappingDict.Remove(assignedTag);
            
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
                
                if(participantBattleActorList.Count == 0 || participantActorEntityList.Count == 0) continue;
                if (battleActorFilter.Count > 0 && !battleActorFilter.Contains(participantBattleActorList[0].battleActor)) continue;
                if(ignoreIfInTaggedEntityMap && EntityInMap(frame, infoEntityRef, participantActorEntityList[0], tem)) continue;

                mappingDict[assignedTag] = participantActorEntityList[0];
                return true;
            }
            return false;
        }
        
        private bool EntityInMap(Frame frame, EntityRef entity, EntityRef participantActorEntity, TaggedEntityMapping* tem)
        {
            var mappings = frame.ResolveDictionary(tem->tagToEntityMap);

            foreach (var tmapping in mappings)
            {
                if (tmapping.Value == participantActorEntity) return true;
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
namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    internal class FindMatchingParticipantEntityRuleNode : RuleNodeBase
    {
        public const string OPTION_ASSIGNEDTAG = "AssignedTag";
        public const string OPTION_BATTLEACTOR = "BattleActor";
        public const string OPTION_TEAMFILTER = "TeamFilter";
        public const string OPTION_IGNORE = "IgnoreIfInMap";
        public const string OPTION_CLEARMAPPINGS = "ClearMappingFirst";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<Tag>(OPTION_ASSIGNEDTAG);
            context.AddOption<BattleActorDefinition>(OPTION_BATTLEACTOR);
            context.AddOption<FindMatchingParticipantEntity.TeamFilterType>(OPTION_TEAMFILTER);
            context.AddOption<bool>(OPTION_IGNORE).WithDisplayName("Ignore If In TaggedEntityMap?");
            context.AddOption<bool>(OPTION_CLEARMAPPINGS).WithDisplayName("Clear Mapping First?").WithDefaultValue(true);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlRule Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(OPTION_BATTLEACTOR).TryGetValue<BattleActorDefinition>(out var battleActorDefinition);
            this.GetNodeOptionByName(OPTION_ASSIGNEDTAG).TryGetValue<Tag>(out var assignedTag);
            this.GetNodeOptionByName(OPTION_TEAMFILTER).TryGetValue<FindMatchingParticipantEntity.TeamFilterType>(out var teamFilter);
            this.GetNodeOptionByName(OPTION_IGNORE).TryGetValue<bool>(out var ignore);
            this.GetNodeOptionByName(OPTION_CLEARMAPPINGS).TryGetValue<bool>(out var clearMappings);
            return new FindMatchingParticipantEntity()
            {
                Label = label,
                assignedTag = assignedTag,
                battleActorFilter = new List<AssetRef<BattleActorDefinition>>()
                {
                    new AssetRef<BattleActorDefinition>(battleActorDefinition),
                },
                teamFilter = teamFilter,
                ignoreIfInTaggedEntityMap = ignore,
                clearMappingFirst = clearMappings
            };
        }
    }
}
#endif