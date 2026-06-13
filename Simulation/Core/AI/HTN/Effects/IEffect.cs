using Quantum;

namespace HnSF.core.AI.HTN.Effects
{
    public interface IEffect
    {
        public string Label { get; set; }
        public EffectType EffectType { get; set; }
        public bool Disable { get; set; }

        public void Apply(ref HTNAgentContext context);
    }
}