using System;
using System.Collections.Generic;
using HnSF.core.state;

namespace Quantum
{
    [System.Serializable]
    public enum MatchParticipantType
    {
        None,
        Player,
        Cpu
    }
    
    [System.Serializable]
    public struct MatchParticipantInitialData
    {
        public int participantId;
        public MatchParticipantType participantType;
        public int playerRef;
        public int localPlayerIndex;
        public bool spectator;
        public bool ready;
        public AssetRef<BattleActorDefinition>[] battleActorDefinitionReferences;
        [NonSerialized] public List<List<AssetRef<HNSFSpecialSet>>> battleActorSpecials;
        public int teamId;
    }
}