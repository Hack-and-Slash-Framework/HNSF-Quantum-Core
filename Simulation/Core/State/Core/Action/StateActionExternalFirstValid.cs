using System;
using System.Linq;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class StateActionExternalFirstValid : HNSFStateAction
    {
        [Serializable]
        public struct ExternalActionWithCondition
        {
            public string Label;
#if QUANTUM_UNITY
            [SerializeReference, SubclassSelector]
#endif
            public HNSFStateDecision[] decisions;
            public HNSFStateActionExternal[] externalActions;
        }
        
        public ExternalActionWithCondition[] externalActionWithConditions = Array.Empty<ExternalActionWithCondition>();
        
        public bool shouldExitEarlyWhenPossible = false;
        public bool returnExitEarlyStatus = false;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            foreach (var externalActionAndConditions in externalActionWithConditions)
            {
                var hasValidDecision = true;
                foreach (var deci in externalActionAndConditions.decisions)
                {
                    if (deci == null) continue;
                    if (!deci.Decide(frame, entity, ref stateContext))
                    {
                        hasValidDecision = false;
                        break;
                    }
                }
                if(hasValidDecision == false) continue;

                foreach (var externalAction in externalActionAndConditions.externalActions)
                {
                    var exitEarly = externalAction.action.Execute(frame, entity,
                        stateContext.agentData, rangePercent, ref stateContext);
                    if (exitEarly && shouldExitEarlyWhenPossible)
                    {
                        if (returnExitEarlyStatus) return true;
                        break;
                    }
                }
                return false;
            }
            return false;
        }
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new StateActionExternalFirstValid());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as StateActionExternalFirstValid;
            t.externalActionWithConditions = externalActionWithConditions.ToArray();
            return base.CopyTo(target);
        }
    }
}