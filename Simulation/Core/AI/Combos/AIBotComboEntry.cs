using Photon.Deterministic;
using UnityEngine.Serialization;

namespace Quantum
{
    [System.Serializable]
    public class AIBotComboEntry : AssetObject
    {
        public string label;
        public FP randomWeighting;
        public FP selectDistanceMin;
        public FP selectDistanceMax;

        public AIBotComboConditionListEntry[] conditionList;
        
        public AIBotComboMoveContainer[] moves;

        public AIBotComboEntry[] potentialFollowups;
    }
}
