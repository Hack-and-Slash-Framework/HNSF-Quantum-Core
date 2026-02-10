using Quantum;

namespace HnSF.core.systems
{
    public unsafe class UpdateActorInput : SystemMainThread, ISignalOnComponentAdded<ActorInputInfo>, ISignalOnComponentRemoved<ActorInputInfo>
    {
        public override void Update(Frame frame)
        {
            var filterPlayerInput = frame.Filter<PlayerLink, ActorInputInfo>();
            while (filterPlayerInput.NextUnsafe(out var entityRef, out var playerLink, out var actorInputInfo))
            {
                var input = frame.GetPlayerInput(playerLink->Player);
                ResolveInput(frame, entityRef, input, actorInputInfo);
            }

            var filterFakeInput = frame.Filter<FakeInput, ActorInputInfo>();
            while (filterFakeInput.NextUnsafe(out var entityRef, out var fakeInput, out var actorInputInfo))
            {
                ResolveInput(frame, entityRef, &fakeInput->frameInput, actorInputInfo);
            }
        }

        public static void ResolveInput(Frame frame, EntityRef entity, Input* input, ActorInputInfo* charaInputs)
        {
            
        }

        private static void IncrementBufferPosition(ActorInputInfo* charaInputs)
        {
            charaInputs->bufferPosition += 1;
            if (charaInputs->bufferPosition == Constants.INPUT_BUFFER_SIZE * 6)
                charaInputs->bufferPosition = Constants.INPUT_BUFFER_SIZE * 5;
        }

        private static void CheckHeldTime(bool inputValue, byte* val)
        {
            if (inputValue)
            {
                if (*val == byte.MaxValue) return;
                *val += 1;
            }
            else
            {
                *val = 0;
            }
        }

        public void OnAdded(Frame f, EntityRef entity, ActorInputInfo* component)
        {
            component->bufferPosition = Constants.INPUT_BUFFER_SIZE * 5;
            
            var inputBuffer = component->inputBuffer;
            var disabledList = component->inputDisabled;

            for(int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
            {
                inputBuffer[i] = new NetworkButtons((int)ActorInputButtonType.NEUTRAL);
            }

            for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
            {
                disabledList[i] = default;
            }
        }

        public void OnRemoved(Frame f, EntityRef entity, ActorInputInfo* component)
        {
        }
    }
}
