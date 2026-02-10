using HnSF.core.state;

namespace Quantum
{
    [System.Serializable]
    public class AIBotComboMoveEntry
    {
        public AIBotComboMoveEntryInputContainer[] inputs;
        public AssetRef<HNSFState>[] expectedState;
    }
}
