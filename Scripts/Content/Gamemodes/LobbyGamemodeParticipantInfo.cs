using System.Collections.Generic;
using System.Linq;
using HnSF.core.state;
using Quantum;

namespace HnSF
{
    [System.Serializable]
    public struct LobbyGamemodeParticipantInfo
    {
        public int participantId;
        public MatchParticipantType participantType;
        public int playerRef;
        public int localPlayerIndex;
        public bool spectator;
        public bool ready;
        public ModAssetSoftReference[] characterReferences;
        public List<List<AssetRef<HNSFSpecialSet>>> characterSpecials;
        public int teamIndex;

        public MatchParticipantInitialData ToGamemodeParticipantInfo()
        {
            var gpi = new MatchParticipantInitialData();
            gpi.participantId = participantId;
            gpi.participantType = participantType;
            gpi.playerRef = playerRef;
            gpi.localPlayerIndex = localPlayerIndex;
            gpi.spectator = spectator;
            gpi.ready = ready;
            gpi.teamId = teamIndex;
            gpi.battleActorDefinitionReferences = new AssetRef<BattleActorDefinition>[characterReferences.Length];
            gpi.battleActorSpecials = new List<List<AssetRef<HNSFSpecialSet>>>();

            for (int i = 0; i < characterReferences.Length; i++)
            {
                var qFighter = HnSFManagersContainer.instance.contentManager.GetAssetFromMod<IFighterDefinition>(characterReferences[i]).GetFighterQuantum();
                gpi.battleActorDefinitionReferences[i] = qFighter;
                var sSet = QuantumUnityDB.GetGlobalAsset(qFighter.statesets[0]);
                gpi.battleActorSpecials.Add(sSet.defaultSpecials.ToList());
            }
            
            return gpi;
        }

        public bool IsValid()
        {
            return participantId > 0;
        }
    }
}