using Quantum;

namespace HnSF.core.state.decisions
{
    [System.Serializable]
    public unsafe partial class HasValidHardTarget : HNSFStateDecision
    {
        public bool inverse;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var combatTargeter)) return false;
            var result = combatTargeter->hardLocked && combatTargeter->targetEntity.IsValid && frame.Exists(combatTargeter->targetEntity);
            return inverse ? !result : result;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasValidHardTarget());
        }
    }
}