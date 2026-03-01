using Photon.Deterministic;

namespace Quantum
{
    public static unsafe partial class InputHelper
    {
        public static NetworkButtons GetButtons(Frame frame, EntityRef entityRef, int offset = 0)
        {
            var actorInputBuffer = frame.Unsafe.GetPointer<ActorInputBuffer>(entityRef);
            var inputBuffer = actorInputBuffer->inputBuffer;

            return inputBuffer[(actorInputBuffer->bufferPosition - offset) % Constants.INPUT_BUFFER_SIZE];
        }

        public static ButtonData GetButtonData(Frame frame, ActorInputBuffer* actorInputInfo,
            ActorInputButtonType buttons,
            int startOffset = 0, int bufferFrames = 0, ButtonDataCheckType checkType = ButtonDataCheckType.ALL,
            bool ignoreDisabledInputs = false)
        {
            if (startOffset + bufferFrames >= Constants.INPUT_BUFFER_SIZE - 1) return default;

            var list = actorInputInfo->inputBuffer;
            var disabledList = actorInputInfo->inputDisabled;

            if (!ignoreDisabledInputs &&
                ButtonEnumHasFlag(disabledList[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE].Bits,
                    (int)buttons)) return default;

            int inputIndex = actorInputInfo->bufferPosition - startOffset;
            for (int i = 0; i < bufferFrames + actorInputInfo->extraBuffer; i++)
            {
                var currentInputIndex = inputIndex - i;
                if (currentInputIndex < 0) break;
                if (!ignoreDisabledInputs &&
                    ButtonEnumHasFlag(disabledList[currentInputIndex % Constants.INPUT_BUFFER_SIZE].Bits, (int)buttons))
                {
                    break;
                }

                if (!AreButtonsFirstPress(list, buttons, currentInputIndex, checkType)) continue;
                return new ButtonData() { WasPressed = true, IsDown = true, WasReleased = false };
            }

            return BuildButtonData(list, buttons, inputIndex, checkType);
        }

        public static bool AreButtonsFirstPress(FixedArray<NetworkButtons> inputList, ActorInputButtonType button,
            int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            return AreButtonsDown(inputList, button, inputIndex, checkType) &&
                   !AreButtonsDown(inputList, button, inputIndex - 1, checkType);
        }

        public static bool AreButtonsDown(FixedArray<NetworkButtons> v, ActorInputButtonType buttonsWantedDown,
            int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            switch (checkType)
            {
                case ButtonDataCheckType.ALL:
                    return ((v)[inputIndex % Constants.INPUT_BUFFER_SIZE].Bits & (int)buttonsWantedDown) ==
                           (int)buttonsWantedDown;
                case ButtonDataCheckType.ANY:
                    return ((v)[inputIndex % Constants.INPUT_BUFFER_SIZE].Bits & (int)buttonsWantedDown) != 0;
                default:
                    return false;
            }
        }

        public static ButtonData BuildButtonData(FixedArray<NetworkButtons> v, ActorInputButtonType buttonsWantedDown,
            int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            bool isFirstPress = AreButtonsFirstPress(v, buttonsWantedDown, inputIndex, checkType);
            bool areButtonsDown = AreButtonsDown(v, buttonsWantedDown, inputIndex, checkType);
            return new ButtonData()
            {
                WasPressed = isFirstPress,
                IsDown = isFirstPress || areButtonsDown,
                WasReleased = !areButtonsDown && AreButtonsDown(v, buttonsWantedDown, inputIndex - 1, checkType)
            };
        }

        public static FPVector2 GetInputQuadrant(FPVector3 input, FPVector3 compareDir)
        {
            input = input.Normalized;
            compareDir = compareDir.Normalized;
            var inputAngle = -FPVector3.SignedAngle(input, compareDir, FPVector3.Up);

            var var90 = (FP)90;
            var varHalf = (FP)45;
            var varFinal = (FP)90 + varHalf;

            if (inputAngle >= -varHalf && inputAngle <= varHalf)
            {
                return FPVector2.Up;
            }
            else if (inputAngle <= -varHalf && inputAngle >= -varFinal)
            {
                return FPVector2.Left;
            }
            else if (inputAngle >= varHalf && inputAngle <= varFinal)
            {
                return FPVector2.Right;
            }

            return FPVector2.Down;
        }

        public static void DisableLastInput(Frame frame, ActorInputBuffer* actorInputInfo)
        {
            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            inputDisabled[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE] =
                inputBuffer[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE];
        }

        public static void DisableInput(Frame frame, ActorInputBuffer* actorInputInfo, ActorInputButtonType buttons)
        {
            var inputDisabled = actorInputInfo->inputDisabled;
            inputDisabled[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE] =
                new NetworkButtons((int)buttons);
        }

        public static void BuildInputFromButtons(ActorInputButtonType inputButtons, ref Input input,
            bool buttonSetValue = true)
        {
        }

#if HNSF_DISABLE_DEFAULTS
#else
        public static byte GetButtonHeldTime(Frame frame, EntityRef actorEntityRef, ActorInputButtonType checkButton)
        {
            if (!frame.Unsafe.TryGetPointer<ActorHoldInputInfo>(actorEntityRef, out ActorHoldInputInfo* holdInputInfo))
                return 0;
            return GetButtonHeldTime(checkButton, holdInputInfo);
        }
        
        public static byte GetButtonHeldTime(ActorInputButtonType inputButtons, ActorHoldInputInfo* actorHoldInputInfo)
        {
            switch (inputButtons)
            {
            }

            return 0;
        }
#endif

        /// <summary>
        /// Make sure input doesn't produce a vector with a magnitude larger than 1. (for example, a keyboard or d-pad)
        /// </summary>
        /// <param name="input">The input movement vector.</param>
        /// <returns>The input with a magnitude no more than 1.</returns>
        public static FPVector2 SquareToCircle(FPVector2 input)
        {
            return (input.SqrMagnitude >= FP._1) ? input.Normalized : input;
        }

        /// <summary>
        /// Make sure input doesn't produce a vector with a magnitude larger than 1. (for example, a keyboard or d-pad)
        /// </summary>
        /// <param name="input">The input movement vector.</param>
        /// <returns>The input with a magnitude no more than 1.</returns>
        public static FPVector3 SquareToCircle(FPVector3 input)
        {
            return (input.SqrMagnitude >= FP._1) ? input.Normalized : input;
        }
    }
}