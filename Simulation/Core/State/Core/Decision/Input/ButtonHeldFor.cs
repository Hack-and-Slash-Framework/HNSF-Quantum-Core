using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class ButtonHeldFor : HNSFStateDecision
    {
        public ActorInputButtonType button;
        public int holdTime;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ActorHoldInputInfo>(entity, out var actorHoldInputInfo)) return false;
            return InputHelper.GetButtonHeldTime(button, actorHoldInputInfo) >= holdTime;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new ButtonHeldFor());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as ButtonHeldFor;
            t.button = button;
            t.holdTime = holdTime;
            return base.CopyTo(target);
        }
    }
}