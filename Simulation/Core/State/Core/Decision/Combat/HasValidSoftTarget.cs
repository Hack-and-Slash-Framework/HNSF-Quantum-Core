using Quantum;

namespace HnSF.core.state.decisions
{
    [System.Serializable]
    public unsafe partial class HasValidSoftTarget : HNSFStateDecision
    {
        public bool inverse;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var combatTargeter)) return false;
            var result = combatTargeter->softTarget.IsValid && frame.Exists(combatTargeter->softTarget);
            return inverse ? !result : result;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasValidSoftTarget());
        }
    }
}