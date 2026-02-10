using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public class AIBotComboMoveContainer
    {
        public FP maximumDistance;
        public int maxWaitTimeForState = 10;
        public AIBotComboMoveEntry move;
    }
}
