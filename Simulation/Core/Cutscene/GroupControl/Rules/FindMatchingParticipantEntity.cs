using System;
using System.Collections.Generic;
using Quantum;

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
        
        public override bool IsValid(Frame frame, EntityRef infoEntityRef)
        {
            frame.AddOrGet<TaggedEntityMapping>(infoEntityRef, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            
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

                mappingDict.Add(assignedTag, participantActorEntityList[0]);
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
