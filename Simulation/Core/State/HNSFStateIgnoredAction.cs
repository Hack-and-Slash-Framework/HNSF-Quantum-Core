using Quantum;

namespace HnSF.core.state
{
    [System.Serializable]
    public struct HNSFStateIgnoredAction
    {
        public AssetRef<HNSFState> stateRef;
        public int actionId;
    }
}
