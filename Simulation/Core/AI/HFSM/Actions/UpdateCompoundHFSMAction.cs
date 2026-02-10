using System;

namespace Quantum.HFSM.Actions.Compound
{
    [Serializable]
    public unsafe partial class UpdateCompoundHFSMAction : AIAction
    {
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return;
            
            var actionAiContext = new AIContext();
            var aiContextUser = new AIContextUser(&compoundAgent->ActionBb, &compoundAgent->Action, AIContextUserType.BattleActorHFSMCompound, null);
            actionAiContext.SetHFSMAgentAndBlackboard(&compoundAgent->Action, entity, &compoundAgent->ActionBb);
            actionAiContext.SetUserData(&aiContextUser);
            if(compoundAgent->Action.Data.Root.IsValid) HFSMManager.Update(frame, frame.DeltaTime, entity, ref actionAiContext);
        }
    }
}