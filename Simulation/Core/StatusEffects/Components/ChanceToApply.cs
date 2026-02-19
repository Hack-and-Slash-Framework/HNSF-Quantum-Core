using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class ChanceToApply : StatusEffectComponent
    {
        public int chance = 100;

        public override bool OnApply(Frame frame)
        {
            return frame.RNG->NextInclusive(0, 100) <= chance;
        }
    }
}