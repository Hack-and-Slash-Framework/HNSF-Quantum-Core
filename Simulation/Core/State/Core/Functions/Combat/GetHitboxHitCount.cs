using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetHitboxHitCount : StateFunctionInt
    {
        public int hitboxIdentifier = -1;
    
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return 0;

            var counter = 0;

            if (hitboxIdentifier < 0)
            {
                counter += boxCombatant->GetCurrentEntityHitCount(frame);
            }
            else
            {
                counter += boxCombatant->GetCurrentEntityHitCount(frame, hitboxIdentifier);
            }
        
            return counter;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetHitboxHitCount());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetHitboxHitCount;
            t.hitboxIdentifier = hitboxIdentifier;
            return base.CopyTo(target);
        }
    }
}