using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class InputButtonDecision : HNSFStateDecision
    {
        public enum ButtonStateType
        {
            IsDown = 0,
            FirstPress = 1,
            Released = 2,
            IsUp = 3
        }

        public bool checkAbilityButton;
        public ActorInputButtonType button;
        public ButtonStateType buttonState;
        public int offset;
        public int buffer;
        public ButtonDataCheckType checkType;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var actorInputs = frame.Unsafe.GetPointer<ActorInputInfo>(entity);
            if (!checkAbilityButton && (actorInputs->ignoreButtons & button) == button) return false; 
            
            var bData = InputHelper.GetButtonData(frame, actorInputs,
                checkAbilityButton ? actorInputs->lastSpecialInput : button,
                offset,
                buffer,
                checkType);

            switch (buttonState)
            {
                case ButtonStateType.IsDown:
                    return bData.IsDown;
                case ButtonStateType.FirstPress:
                    return bData.WasPressed;
                case ButtonStateType.Released:
                    return bData.WasReleased;
                case ButtonStateType.IsUp:
                    return !bData.IsDown;
            }

            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new InputButtonDecision());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as InputButtonDecision;
            t.checkAbilityButton = this.checkAbilityButton;
            t.button = this.button;
            t.buttonState = this.buttonState;
            t.offset = this.offset;
            t.buffer = this.buffer;
            t.checkType = this.checkType;
            return base.CopyTo(target);
        }
    }
}