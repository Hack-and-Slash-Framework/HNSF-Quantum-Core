using System;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class RequestStateChangeByTag : HNSFStateAction
    {
        [Serializable]
        public class StateChangeEntry
        {
            public string name;
#if QUANTUM_UNITY
            [SerializeReference, SubclassSelector]
#endif
            public HNSFStateDecision[] condition;
            public bool overrideMoveset = false;
            [DrawIf(nameof(overrideMoveset), true)]
            public AssetRef<Tag> toStateMovesetTag;
            public AssetRef<Tag> toStateTag;
            public bool transitionToSameFrame;
            [DrawIf(nameof(transitionToSameFrame), false)]
            public int toFrame = 1;
            public bool overrideTransitionRequests;
            public bool checkToStateConditions = true;
            public bool checkToStateInputCondition;
        }
        
        public StateChangeEntry[] stateChangeList;
        public bool immediateTransition;
        public int throweeId;
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;

            DoAction(frame, targetEntityRef, ref targetStateContext);
            if (actionTargetContext.targetType == StateActionTargetType.Self)
            {
                stateContext = targetStateContext;
                if (immediateTransition) return true;
            }
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !frame.TryFindAsset(csm->stateAgent.stateData.state, out var currentState)) return;
            
            var savedWorkingState = stateContext.workingState;
            foreach (var entry in stateChangeList)
            {
                if (!stateSet.AttemptGetStateByTag(entry.overrideMoveset ? entry.toStateMovesetTag : csm->stateAgent.stateData.moveset, entry.toStateTag, out var toStateRef))
                {
                    continue;
                }
                stateContext.workingState = toStateRef;
                bool conditionsValid = true;
                foreach (var deci in entry.condition)
                {
                    if(deci == null) continue;
                    if (deci.Decide(frame, entity, ref stateContext)) continue;
                    conditionsValid = false;
                    break;
                }
                if (conditionsValid == false) continue;
                if (!frame.TryFindAsset(toStateRef.Id, out HNSFState toState)) continue;
                if (!entry.overrideTransitionRequests 
                    && stateContext.agentData->toStateRequested) continue;
                if (entry.checkToStateConditions
                    && toState.CheckConditions(frame, entity, ref stateContext) == false) continue;
                
                if (entry.checkToStateInputCondition 
                    && toState.defaultInputConditions.Length > 0 
                    && frame.Unsafe.TryGetPointer<ActorInputBuffer>(entity, out var actorInputInfo) )
                {
                    var gotValidInput = false;
                    foreach (var ic in toState.defaultInputConditions)
                    {
                        if(frame.TryFindAsset(ic, out var inputConditionListAsset) == false
                           || InputHelper.CheckInputConditions(frame, actorInputInfo, inputConditionListAsset.conditions, actorInputInfo->bufferPosition) == -1) continue;
                        gotValidInput = true;
                        break;
                    }
                    if (gotValidInput == false) continue;
                }
                
                stateContext.agentData->toStateRequested = true;
                stateContext.agentData->toState = toStateRef;
                stateContext.agentData->toFrame = entry.transitionToSameFrame ? stateContext.agentData->frame : entry.toFrame;
            }
            stateContext.workingState = savedWorkingState;

            if (immediateTransition)
            {
                HNSFStateHelper.Generic.CheckForStateChange(frame, entity, csm, stateContext.aiConfig);
            }
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new RequestStateChangeByTag());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as RequestStateChangeByTag;
            t.stateChangeList = new StateChangeEntry[stateChangeList.Length];
            for (int i = 0; i < stateChangeList.Length; i++)
            {
                t.stateChangeList[i] = new StateChangeEntry();
                t.stateChangeList[i].name = stateChangeList[i].name;
                t.stateChangeList[i].condition = new HNSFStateDecision[stateChangeList[i].condition.Length];
                for (int a = 0; a < stateChangeList[i].condition.Length; a++)
                {
                    t.stateChangeList[i].condition[a] = stateChangeList[i].condition[a].Copy();
                }
                t.stateChangeList[i].toStateMovesetTag = stateChangeList[i].toStateMovesetTag;
                t.stateChangeList[i].toStateTag = stateChangeList[i].toStateTag;
                t.stateChangeList[i].toFrame = stateChangeList[i].toFrame;
                t.stateChangeList[i].overrideTransitionRequests = stateChangeList[i].overrideTransitionRequests;
                t.stateChangeList[i].checkToStateConditions = stateChangeList[i].checkToStateConditions;
                t.stateChangeList[i].transitionToSameFrame = stateChangeList[i].transitionToSameFrame;
            }
            t.immediateTransition = immediateTransition;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}