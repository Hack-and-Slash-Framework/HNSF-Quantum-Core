using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class StateActionListExternal : AssetObject
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateAction[] actions = Array.Empty<HNSFStateAction>();
        public bool shouldExitEarlyWhenPossible = false;
        
        public virtual bool Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)) return false;

            var sc = new HNSFStateContext(frame, entity);
            
            if (!CheckConditions(frame, entity, ref sc)) return false;
            
            foreach (var act in actions)
            {
                var exitEarly = act.Execute(frame, entity, &gsm->stateAgent.stateData, 0, ref sc);
                if (exitEarly && shouldExitEarlyWhenPossible) return true;
            }
            return false;
        }
        
        public bool CheckConditions(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (conditions == null || conditions.Length == 0) return true;
            foreach (var d in conditions)
            {
                if (d.Decide(frame, entity, ref stateContext) == false) return false;
            }
            return true;
        }
    }
}