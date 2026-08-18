using System;

namespace Quantum
{
    [Serializable]
    public unsafe partial class HFSMModifyInputAction : ExecuteIfConditionAction
    {
        public enum ModifyType
        {
            Set,
            Add,
            Remove
        }

        public ModifyType modifyType;
        public ActorInputButtonType input;
        
        public override void ConditionalExecute(Frame frame, EntityRef entity, ref AIContext aiContext, AIContextUser* user,
            AIConfig aiConfig)
        {
            /*
            if (!frame.Unsafe.TryGetPointer<DummyConfiguration>(entity, out var dummyConfig)) return;
            
            switch (modifyType)
            {
                case ModifyType.Set:
                    dummyConfig->frameInput.Clear();
                    InputHelper.BuildInputFromButtons(input, ref dummyConfig->frameInput);
                    break;
                case ModifyType.Add:
                    InputHelper.BuildInputFromButtons(input, ref dummyConfig->frameInput);
                    break;
                case ModifyType.Remove:
                    InputHelper.BuildInputFromButtons(input, ref dummyConfig->frameInput, false);
                    break;
            }*/
        }
    }
}