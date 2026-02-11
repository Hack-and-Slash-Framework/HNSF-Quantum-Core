using Quantum;

namespace HnSF.core.systems
{
    public unsafe partial class UpdateActorInput : SystemMainThread, ISignalOnComponentAdded<ActorInputBuffer>, ISignalOnComponentRemoved<ActorInputBuffer>
    {
        public override void Update(Frame frame)
        {
            var filterPlayerInput = frame.Filter<PlayerLink, ActorInputBuffer>();
            while (filterPlayerInput.NextUnsafe(out var entityRef, out var playerLink, out var actorInputInfo))
            {
                var input = frame.GetPlayerInput(playerLink->Player);
                ResolveInput(frame, entityRef, input, actorInputInfo);
            }

            var filterFakeInput = frame.Filter<FakeInput, ActorInputBuffer>();
            while (filterFakeInput.NextUnsafe(out var entityRef, out var fakeInput, out var actorInputInfo))
            {
                ResolveInput(frame, entityRef, &fakeInput->frameInput, actorInputInfo);
            }
        }

        static partial void ResolveInput(Frame frame, EntityRef entity, Input* input, ActorInputBuffer* charaInputs);
        static partial void ClearBufferItem(Frame frame, FixedArray<NetworkButtons> inputBuffer, int index);

        private static void IncrementBufferPosition(ActorInputBuffer* charaInputs)
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

        public void OnAdded(Frame f, EntityRef entity, ActorInputBuffer* component)
        {
            component->bufferPosition = Constants.INPUT_BUFFER_SIZE * 5;
            
            var inputBuffer = component->inputBuffer;
            var disabledList = component->inputDisabled;

            for(int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
            {
                ClearBufferItem(f, inputBuffer, i);
            }

            for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
            {
                disabledList[i] = default;
            }
        }

        public void OnRemoved(Frame f, EntityRef entity, ActorInputBuffer* component)
        {
        }
    }
}
