using System;

namespace Quantum.HFSM.Actions.Compound
{
    [Serializable]
    public unsafe partial class CompoundHFSMSetAction : AIAction
    {
        public AIParamAssetRef hfsmAction;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!frame.Unsafe.TryGetPointer<HFSMCompoundAgent>(entity, out var compoundAgent)) return;
            var userData = (AIContextUser*)aiContext.UserData;
            
            frame.TryFindAsset(userData->HFSMAgent->Config.Id, out AIConfigBase aiConfig);
            frame.TryFindAsset(hfsmAction.Resolve(frame, entity, userData->Blackboard, aiConfig, ref aiContext), out HFSMRoot hfsmRoot);

            var actionContext = aiContext;
            var aiContextUserAction = new AIContextUser(&compoundAgent->ActionBb, &compoundAgent->Action, AIContextUserType.BattleActorHFSMCompound, null);
            
            compoundAgent->Action.Data = new HFSMData();
            compoundAgent->Action.Data.Root = hfsmRoot;
            
            actionContext.SetHFSMAgentAndBlackboard(&compoundAgent->Action, entity, &compoundAgent->ActionBb);
            actionContext.SetUserData(&aiContextUserAction);
            HFSMManager.Init(frame, entity, frame.FindAsset(compoundAgent->Action.Data.Root), ref actionContext);
        }
    }
}