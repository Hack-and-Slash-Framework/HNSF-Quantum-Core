using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF
{
    public unsafe class EventActionCallExternalStateActions : HNSFEventAction
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();
        
        public AssetRef<HNSFStateActionExternal>[] externalActions = Array.Empty<AssetRef<HNSFStateActionExternal>>();
        public bool shouldExitEarlyWhenPossible = false;
        
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)) return;

            var sc = new HNSFStateContext(frame, entity);
            
            if (!CheckConditions(frame, entity, ref sc)) return;
            
            foreach (var externalActionAssetRef in externalActions)
            {
                if(!frame.TryFindAsset(externalActionAssetRef, out var externalAction)) continue;
                var exitEarly = externalAction.action.Execute(frame, entity, &gsm->stateAgent.stateData, 0, ref sc);
                if (exitEarly && shouldExitEarlyWhenPossible) break;
            }
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