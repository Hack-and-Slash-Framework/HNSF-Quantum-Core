using System;

namespace Quantum.HFSM.Actions
{
    [Serializable]
    public unsafe partial class ModifyActorInput : ExecuteIfConditionAction
    {
        public enum ModifyType
        {
            Set,
            Add,
            Remove
        }

        public ModifyType modifyType;
        public ActorInputButtonType input;
        public int modifyEvery = 0;
        
        public override void ConditionalExecute(Frame frame, EntityRef entity, ref AIContext aiContext, AIContextUser* user, AIConfig aiConfig)
        {
            if (modifyEvery > 0 && frame.Number % modifyEvery != 0) return;
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(entity, out var battleActorAI)
                || !frame.Unsafe.TryGetPointer<FakeInput>(battleActorAI->target, out var fakeInput)) return;
            
            switch (modifyType)
            {
                case ModifyType.Set:
                    fakeInput->frameInput.Clear();
                    InputHelper.BuildInputFromButtons(input, ref fakeInput->frameInput);
                    break;
                case ModifyType.Add:
                    InputHelper.BuildInputFromButtons(input, ref fakeInput->frameInput);
                    break;
                case ModifyType.Remove:
                    InputHelper.BuildInputFromButtons(input, ref fakeInput->frameInput, false);
                    break;
            }
        }
    }
}