using HnSF.StatusEffects;
using Quantum;

namespace HnSF.core.state.decisions
{
    [System.Serializable]
    public unsafe partial class HasStatusEffect : HNSFStateDecision
    {
        public AssetRef<StatusEffectAsset> statusEffect;
        public bool inverse;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<StatusEffectActor>(entity, out var statusEffectActor)) return inverse;
            var result = statusEffectActor->HasStatusEffect(frame, statusEffect);
            return inverse ? !result : result;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasStatusEffect());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HasStatusEffect;
            t.statusEffect = statusEffect;
            t.inverse = inverse;
            return base.CopyTo(target);
        }
    }
}