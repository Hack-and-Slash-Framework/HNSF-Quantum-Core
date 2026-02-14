namespace Quantum
{
    public static unsafe partial class InputHelper
    {
        public static int CheckInputConditions(Frame frame, ActorInputBuffer* actorInputBuffer,
            InputCondition[] conditions, int lastBufferPos)
        {
            for (var i = conditions.Length - 1; i >= 0; i--)
            {
                lastBufferPos = CheckInputCondition(frame, actorInputBuffer, conditions[i],
                    bufferStartPosition: lastBufferPos);
                if (lastBufferPos == -1) break;
            }

            return lastBufferPos;
        }

        public static bool CheckInputConditionsResult(Frame frame, ActorInputBuffer* actorInputBuffer,
            InputCondition[] conditions)
        {
            int lastBufferPos = actorInputBuffer->bufferPosition;
            for (int i = conditions.Length - 1; i >= 0; i--)
            {
                lastBufferPos = CheckInputCondition(frame, actorInputBuffer, conditions[i],
                    bufferStartPosition: lastBufferPos);
                if (lastBufferPos == -1) return false;
            }

            return true;
        }

        public static int CheckInputCondition(Frame frame, ActorInputBuffer* actorInputBuffer, InputCondition condition,
            int bufferStartPosition)
        {
            if (condition.sequence.Length == 0) return -1;

            switch (condition.method)
            {
                case EnterInputMethod.Normal:
                    return CheckInputSequence(actorInputBuffer, condition.sequence, condition.impreciseInputCount,
                        !condition.ignoreDisableInput, bufferStartPosition,
                        actorInputBuffer->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.Strict:
                    return CheckInputSequenceStrict(actorInputBuffer, condition.sequence,
                        condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition,
                        actorInputBuffer->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.Once:
                    return CheckInputSequenceOnce(actorInputBuffer, condition.sequence,
                        condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition,
                        actorInputBuffer->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                case EnterInputMethod.OnceStrict:
                    return CheckInputSequenceOnceStrict(actorInputBuffer, condition.sequence,
                        condition.impreciseInputCount, !condition.ignoreDisableInput, bufferStartPosition,
                        actorInputBuffer->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1);
                default:
                    return -1;
            }
        }

        public static bool ButtonEnumHasFlag(int source, int input)
        {
            return (source & input) == input;
        }

        public static int CheckInputSequence(ActorInputBuffer* actorInputBuffer, InputBitmask[] sequence,
            int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int sequencesIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            bool noMatches = true;

            var inputDisabled = actorInputBuffer->inputDisabled;
            var inputBuffer = actorInputBuffer->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;

                if (sequencesIndex == -1) // Input sequence successfully read.
                    return i;

                if (actorInputBuffer->ignoreButtons.IsFlagSet(sequence[sequencesIndex].input)) return -1;

                if (inputAllowDisable && noMatches && inputDisabled[listIndex].Bits != 0 &&
                    ButtonEnumHasFlag(inputDisabled[listIndex].Bits,
                        inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                int neededInput = (int)sequence[sequencesIndex].input;
                if (framesSinceLastMatch > sequence[sequencesIndex].lenience + actorInputBuffer->extraBuffer)
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

        public static int CheckInputSequenceStrict(ActorInputBuffer* actorInputBuffer,
            InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition,
            int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            int impreciseMatches = 0;
            bool noMatches = true;

            var inputDisabled = actorInputBuffer->inputDisabled;
            var inputBuffer = actorInputBuffer->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;

                if (inputIndex == -1) // Input sequence successfully read.
                    return i;

                if (actorInputBuffer->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;

                if (inputAllowDisable && noMatches && inputDisabled[listIndex].Bits != 0 &&
                    ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                int neededInput = (int)sequence[inputIndex].input;
                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputBuffer->extraBuffer)
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

        public static int CheckInputSequenceOnce(ActorInputBuffer* actorInputBuffer, InputBitmask[] sequence,
            int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition, int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;

            var inputDisabled = actorInputBuffer->inputDisabled;
            var inputBuffer = actorInputBuffer->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;

                if (inputAllowDisable && inputDisabled[listIndex].Bits != 0 &&
                    ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;


                if (inputIndex == -1) // Input match successful, check for button release.
                {
                    if (ButtonEnumHasFlag(inputBuffer[listIndex].Bits, (int)sequence[0].input) == false)
                        return i;
                    if (framesSinceLastMatch > sequence[0].lenience + actorInputBuffer->extraBuffer)
                        return -1;
                    framesSinceLastMatch++;
                    continue;
                }

                if (actorInputBuffer->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;

                int neededInput = (int)sequence[inputIndex].input;
                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputBuffer->extraBuffer)
                    return -1;
                framesSinceLastMatch++;

                if (ButtonEnumHasFlag(inputBuffer[listIndex].Bits, neededInput)) // Input matches.
                {
                    inputIndex--;
                    framesSinceLastMatch = 0;
                    //i--;
                }
            }

            return -1;
        }

        public static int CheckInputSequenceOnceStrict(ActorInputBuffer* actorInputBuffer,
            InputBitmask[] sequence, int impreciseInputCount, bool inputAllowDisable, int bufferStartPosition,
            int bufferEndPosition)
        {
            int inputIndex = sequence.Length - 1;
            int framesSinceLastMatch = 0;
            int impreciseMatches = 0;

            var inputDisabled = actorInputBuffer->inputDisabled;
            var inputBuffer = actorInputBuffer->inputBuffer;

            for (int i = bufferStartPosition; i >= bufferEndPosition; i--)
            {
                var listIndex = i % Constants.INPUT_BUFFER_SIZE;

                if (inputAllowDisable && inputDisabled[listIndex].Bits != 0 &&
                    ButtonEnumHasFlag(inputDisabled[listIndex].Bits, inputBuffer[listIndex].Bits)) // Hit buffer limit.
                    return -1;

                if (inputIndex == -1) // Input match successful, check for button release.
                {
                    if ((inputBuffer[listIndex].Bits ^ (int)sequence[0].input) << 27 !=
                        0) // Input was released before the press.
                        return i;
                    if (framesSinceLastMatch > sequence[0].lenience + actorInputBuffer->extraBuffer)
                        return -1;
                    framesSinceLastMatch++;
                    continue;
                }

                if (actorInputBuffer->ignoreButtons.IsFlagSet(sequence[inputIndex].input)) return -1;

                int neededInput = (int)sequence[inputIndex].input;

                if (framesSinceLastMatch > sequence[inputIndex].lenience + actorInputBuffer->extraBuffer)
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
    }
}