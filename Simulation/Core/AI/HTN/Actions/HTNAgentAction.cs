using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using Quantum;

namespace HnSF.core.AI.HTN.Actions
{
    [Serializable]
    public unsafe partial class HTNAgentAction : GroupControlAction
    {
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            base.OnEnter(frame, infoEntityRef, ref context);
            
            var htnContext = (HTNAgentContext*)context.CustomData;
            if (htnContext == null)
                return;

            htnContext->agent->currentActionResult = HTNTaskResult.PROCESSING;
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            return base.Tick(frame, infoEntityRef, ref context);
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            base.OnExit(frame, infoEntityRef, ref context);
            
            var htnContext = (HTNAgentContext*)context.CustomData;
            if (htnContext != null)
            {
                htnContext->agent->currentActionResult = HTNTaskResult.SUCCESS;
            }
        }
    }
}