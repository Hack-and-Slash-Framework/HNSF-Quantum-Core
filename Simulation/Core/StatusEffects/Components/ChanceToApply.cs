using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class ChanceToApply : StatusEffectComponent
    {
#if QUANTUM_UNITY
        [RangeEx(0, 100)]
#endif
        public int chance = 100;
        
        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return frame.RNG->NextInclusive(1, 100) <= chance;
        }
    }
}