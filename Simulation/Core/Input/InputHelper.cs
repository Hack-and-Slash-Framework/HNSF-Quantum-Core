using Photon.Deterministic;

namespace Quantum
{
    public unsafe class InputHelper
    {
        public static NetworkButtons GetButtons(Frame frame, EntityRef entityRef, int offset = 0)
        {
            var actorInputInfo = frame.Unsafe.GetPointer<ActorInputInfo>(entityRef);
            var inputBuffer = actorInputInfo->inputBuffer;

            return inputBuffer[(actorInputInfo->bufferPosition - offset) % Constants.INPUT_BUFFER_SIZE];
        }
        
        public static ButtonData GetButtonData(Frame frame, ActorInputInfo* actorInputInfo, ActorInputButtonType buttons, 
            int startOffset = 0, int bufferFrames = 0, ButtonDataCheckType checkType = ButtonDataCheckType.ALL, bool ignoreDisabledInputs = false)
        {
            if(startOffset + bufferFrames >= Constants.INPUT_BUFFER_SIZE-1) return default;
            
            var list = actorInputInfo->inputBuffer;
            var disabledList = actorInputInfo->inputDisabled;
            
            if (!ignoreDisabledInputs &&
                ButtonEnumHasFlag(disabledList[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE].Bits, (int)buttons)) return default;
            
            int inputIndex = actorInputInfo->bufferPosition - startOffset;
            for (int i = 0; i < bufferFrames + actorInputInfo->extraBuffer; i++)
            {
                var currentInputIndex = inputIndex - i;
                if (currentInputIndex < 0) break;
                if (!ignoreDisabledInputs && ButtonEnumHasFlag(disabledList[currentInputIndex % Constants.INPUT_BUFFER_SIZE].Bits, (int)buttons))
                {
                    break;
                }
                
                if (!AreButtonsFirstPress(list, buttons, currentInputIndex, checkType)) continue;
                return new ButtonData() { WasPressed = true, IsDown = true, WasReleased = false };
            }
            
            return BuildButtonData(list, buttons, inputIndex, checkType);
        }

        public static bool AreButtonsFirstPress(FixedArray<NetworkButtons> inputList, ActorInputButtonType button, int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            return AreButtonsDown(inputList, button, inputIndex, checkType) && !AreButtonsDown(inputList, button, inputIndex - 1, checkType);
        }
        
        public static bool AreButtonsDown(FixedArray<NetworkButtons> v, ActorInputButtonType buttonsWantedDown, int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            switch (checkType)
            {
                case ButtonDataCheckType.ALL:
                    return ((v)[inputIndex % Constants.INPUT_BUFFER_SIZE].Bits & (int)buttonsWantedDown) == (int)buttonsWantedDown;
                case ButtonDataCheckType.ANY:
                    return ((v)[inputIndex % Constants.INPUT_BUFFER_SIZE].Bits & (int)buttonsWantedDown) != 0;
                default:
                    return false;
            }
        }

        public static ButtonData BuildButtonData(FixedArray<NetworkButtons> v, ActorInputButtonType buttonsWantedDown, int inputIndex, ButtonDataCheckType checkType = ButtonDataCheckType.ALL)
        {
            bool isFirstPress = AreButtonsFirstPress(v, buttonsWantedDown, inputIndex, checkType);
            bool areButtonsDown = AreButtonsDown(v, buttonsWantedDown, inputIndex, checkType);
            return new ButtonData()
            {
                WasPressed = isFirstPress,
                IsDown = isFirstPress || areButtonsDown,
                WasReleased = !areButtonsDown && AreButtonsDown(v, buttonsWantedDown, inputIndex-1, checkType)
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
            }else if (inputAngle <= -varHalf && inputAngle >= -varFinal)
            {
                return FPVector2.Left;
            }else if (inputAngle >= varHalf && inputAngle <= varFinal)
            {
                return FPVector2.Right;
            }

            return FPVector2.Down;
        }

        public static int CheckInputConditions(Frame frame, ActorInputInfo* actorInputInfo, InputCondition[] conditions, int lastBufferPos)
        {
            for(var i = conditions.Length-1; i >= 0; i--)
            {
                lastBufferPos = CheckInputCondition(frame, actorInputInfo, conditions[i], bufferStartPosition: lastBufferPos);
                if (lastBufferPos == -1) break;
            }
            return lastBufferPos;
        }
        
        public static bool CheckInputConditionsResult(Frame frame, ActorInputInfo* actorInputInfo, InputCondition[] conditions)
        {
            int lastBufferPos = actorInputInfo->bufferPosition;
            for(int i = conditions.Length-1; i >= 0; i--)
            {
                lastBufferPos = CheckInputCondition(frame, actorInputInfo, conditions[i], bufferStartPosition: lastBufferPos);
                if (lastBufferPos == -1) return false;
            }
            return true;
        }

        public static int CheckInputCondition(Frame frame, ActorInputInfo* actorInputInfo, InputCondition condition, int bufferStartPosition)
        {
            if (condition.sequence.Length == 0) return -1;
            
            switch (condition.method)
            {
                case EnterInputMethod.Normal:
                    return CheckInputSequence(frame, actorInputInfo, condition.sequence, condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition, actorInputInfo->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.Strict:
                    return CheckInputSequenceStrict(frame, actorInputInfo, condition.sequence, condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition, actorInputInfo->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.Once:
                    return CheckInputSequenceOnce(frame, actorInputInfo, condition.sequence, condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition, actorInputInfo->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.OnceStrict:
                    return CheckInputSequenceOnceStrict(frame, actorInputInfo, condition.sequence, condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition, actorInputInfo->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                default:
                    return -1;
            }
        }
        
        public static bool ButtonEnumHasFlag(int source, int input)
        {
            return (source & input) == input;
        }
        
        public static int CheckInputSequence(Frame frame, ActorInputInfo* actorInputInfo, InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int sequencesIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            bool noMatches = true;

            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;
                
                if (sequencesIndex == -1) // Input sequence successfully read.
                    return i;
                
                if (actorInputInfo->ignoreButtons.IsFlagSet(sequence[sequencesIndex].input)) return -1;
                
                if (inputAllowDisable && noMatches && inputDisabled[listIndex].Bits != 0 &&
                    ButtonEnumHasFlag(inputDisabled[listIndex].Bits,
                        inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                int neededInput = (int)sequence[sequencesIndex].input;
                if (framesSinceLastMatch > sequence[sequencesIndex].lenience + actorInputInfo->extraBuffer)
                    return -1;
                framesSinceLastMatch++;
                
                if ((inputBuffer[listIndex].Bits & neededInput) == neededInput) // Input matches.
                {
                    noMatches = false;
                    sequencesIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }
            }

            return -1;
        }

        public static int CheckInputSequenceStrict(Frame frame, ActorInputInfo* actorInputInfo, InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            int impreciseMatches = 0;
            bool noMatches = true;

            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;
                
                if (inputIndex == -1) // Input sequence successfully read.
                    return i;
                
                if (actorInputInfo->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;
                
                if (inputAllowDisable && noMatches && inputDisabled[listIndex].Bits != 0 && ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                int neededInput = (int)sequence[inputIndex].input;
                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputInfo->extraBuffer)
                    return -1;
                framesSinceLastMatch++;

                if ((inputBuffer[listIndex].Bits ^ neededInput) << 27 == 0) // Input matches.
                {
                    noMatches = false;
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }

                if ((inputBuffer[listIndex].Bits & neededInput) == neededInput) // Input doesn't match precisely.
                {
                    noMatches = false;
                    if (impreciseMatches >= impreciseInputCount)
                        continue;
                    impreciseMatches++;
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }
            }

            return -1;
        }

        public static int CheckInputSequenceOnce(Frame frame, ActorInputInfo* actorInputInfo, InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;

            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;
                
                if (inputAllowDisable && inputDisabled[listIndex].Bits != 0 && ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                
                if (inputIndex == -1) // Input match successful, check for button release.
                {
                    if(ButtonEnumHasFlag(inputBuffer[listIndex].Bits, (int)sequence[0].input) == false)
                        return i;
                    if(framesSinceLastMatch > sequence[0].lenience + actorInputInfo->extraBuffer)
                        return -1;
                    framesSinceLastMatch++;
                    continue;
                }
                
                if (actorInputInfo->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;
                
                int neededInput = (int)sequence[inputIndex].input;
                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputInfo->extraBuffer)
                    return -1;
                framesSinceLastMatch++;

                if(ButtonEnumHasFlag(inputBuffer[listIndex].Bits, neededInput)) // Input matches.
                {
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }
            }

            return -1;
        }

        public static int CheckInputSequenceOnceStrict(Frame frame, ActorInputInfo* actorInputInfo, InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            int impreciseMatches = 0;

            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;
                
                if (inputAllowDisable && inputDisabled[listIndex].Bits != 0 && ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                if (inputIndex == -1) // Input match successful, check for button release.
                {
                    if ((inputBuffer[listIndex].Bits ^ (int)sequence[0].input) << 27 != 0) // Input was released before the press.
                        return i;
                    if (framesSinceLastMatch > sequence[0].lenience + actorInputInfo->extraBuffer)
                        return -1;
                    framesSinceLastMatch++;
                    continue;
                }
                
                if (actorInputInfo->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;
                
                int neededInput = (int)sequence[inputIndex].input;

                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputInfo->extraBuffer)
                    return -1;
                framesSinceLastMatch++;

                if ((inputBuffer[listIndex].Bits ^ neededInput) << 27 == 0) // Input matches.
                {
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                    continue;
                }

                if ((inputBuffer[listIndex].Bits & neededInput) == neededInput) // Input matches imprecisely.
                {
                    if (impreciseMatches >= impreciseInputCount)
                        continue;
                    impreciseMatches++;
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }
            }

            return -1;
        }

        public static void DisableLastInput(Frame frame, ActorInputInfo* actorInputInfo)
        {
            var inputDisabled = actorInputInfo->inputDisabled;
            var inputBuffer = actorInputInfo->inputBuffer;

            inputDisabled[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE] = inputBuffer[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE];
        }

        public static void DisableInput(Frame frame, ActorInputInfo* actorInputInfo, ActorInputButtonType buttons)
        {
            var inputDisabled = actorInputInfo->inputDisabled;
            inputDisabled[actorInputInfo->bufferPosition % Constants.INPUT_BUFFER_SIZE] = new NetworkButtons((int)buttons);
        }

        public static void BuildInputFromButtons(ActorInputButtonType inputButtons, ref Input input, bool buttonSetValue = true)
        {
        }

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

        /// <summary>
        /// Make sure input doesn't produce a vector with a magnitude larger than 1. (for example, a keyboard or d-pad)
        /// </summary>
        /// <param name="input">The input movement vector.</param>
        /// <returns>The input with a magnitude no more than 1.</returns>
        public FPVector2 SquareToCircle(FPVector2 input)
        {
            return (input.SqrMagnitude >= FP._1) ? input.Normalized : input;
        }
        
        /// <summary>
        /// Make sure input doesn't produce a vector with a magnitude larger than 1. (for example, a keyboard or d-pad)
        /// </summary>
        /// <param name="input">The input movement vector.</param>
        /// <returns>The input with a magnitude no more than 1.</returns>
        public FPVector3 SquareToCircle(FPVector3 input)
        {
            return (input.SqrMagnitude >= FP._1) ? input.Normalized : input;
        }
    }
}
