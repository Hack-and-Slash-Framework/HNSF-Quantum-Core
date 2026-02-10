using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class StateActionExternal : HNSFStateAction
    {
        public HNSFStateActionExternal[] externalActions = Array.Empty<HNSFStateActionExternal>();
        public bool shouldExitEarlyWhenPossible = false;
        public bool returnExitEarlyStatus = false;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            foreach (var externalAction in externalActions)
            {
                var exitEarly = externalAction.action.Execute(frame, entity, stateContext.agentData, rangePercent, ref stateContext);
                if (exitEarly && shouldExitEarlyWhenPossible)
                {
                    if (returnExitEarlyStatus) return true;
                    break;
                }
            }
            return false;
        }
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new StateActionExternal());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as StateActionExternal;
            t.externalActions = new HNSFStateActionExternal[externalActions.Length];
            Array.Copy(externalActions, t.externalActions, externalActions.Length);
            t.shouldExitEarlyWhenPossible = shouldExitEarlyWhenPossible;
            t.returnExitEarlyStatus = returnExitEarlyStatus;
            return base.CopyTo(target);
        }
    }
}
