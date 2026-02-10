using Quantum;
using Quantum.BotSDK;

namespace HnSF.core.systems
{
    public class AIActorInitialization : SystemSignalsOnly, ISignalOnComponentAdded<HFSMCompoundAgent>, ISignalOnComponentRemoved<HFSMCompoundAgent>
    {
        public unsafe void OnAdded(Frame f, EntityRef entity, HFSMCompoundAgent* component)
        {
            var aiContext = new AIContext();
            var aiContextUserBrain = new AIContextUser(&component->BrainBb, &component->Brain, AIContextUserType.BattleActorHFSMCompound, null);
            
            if (component->Brain.Data.Root.IsValid)
            {
                aiContext.SetHFSMAgentAndBlackboard(&component->Brain, entity, &component->BrainBb);
                aiContext.SetUserData(&aiContextUserBrain);
                HFSMManager.Init(f, entity, f.FindAsset(component->Brain.Data.Root), ref aiContext);
            }
            
            var aiContextUserAction = new AIContextUser(&component->ActionBb, &component->Action, AIContextUserType.BattleActorHFSMCompound, null);
            if (component->Action.Data.Root.IsValid)
            {
                aiContext.SetHFSMAgentAndBlackboard(&component->Action, entity, &component->ActionBb);
                aiContext.SetUserData(&aiContextUserAction);
                HFSMManager.Init(f, entity, f.FindAsset(component->Action.Data.Root), ref aiContext);
            }
            BotSDKDebuggerSystem.AddToDebugger(f, entity, component);
        }

        public unsafe void OnRemoved(Frame f, EntityRef entity, HFSMCompoundAgent* component)
        {
            component->ActionBb.Free(f);
            component->BrainBb.Free(f);
            component->ActionBb = default;
            component->BrainBb = default;
        }
    }
}