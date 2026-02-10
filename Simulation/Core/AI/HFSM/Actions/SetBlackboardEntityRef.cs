using System;

namespace Quantum.HFSM.Actions
{
    [Serializable]
    public unsafe partial class SetBlackboardEntityRef : AIAction
    {
        [BotSDKTooltip(Text = "Link this slot with a Key slot from a Blackboard node.")]
        public AIBlackboardValueKey Key;
        [BotSDKTooltip(Text = "EntityRef to set the blackboard value to.")]
        public AIParamEntityRef Value;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var aiContextUser = ((AIContextUser*)aiContext.UserData);
            AIConfigBase aiConfig = frame.FindAsset(aiContextUser->HFSMAgent->Config);

            var val = Value.Resolve(frame, entity, aiContextUser->Blackboard, aiConfig, ref aiContext);
            aiContextUser->Blackboard->Set(frame, Key.Key, val);
        }
    }
}