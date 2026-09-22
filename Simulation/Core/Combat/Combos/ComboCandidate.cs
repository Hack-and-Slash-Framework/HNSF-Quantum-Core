using Photon.Deterministic;

namespace HnSF.core.GroupControl.Combo
{
    public struct ComboCandidate
    {
        public int scriptIndex;
        public ComboBattleScript comboScript;
        public FP weight;
    }
}