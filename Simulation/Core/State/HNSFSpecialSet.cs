using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state
{
    public unsafe partial class HNSFSpecialSet : AssetObject
    {
        [System.Serializable]
        public class StateWithCondition
        {
            public string name;
#if QUANTUM_UNITY
            [SerializeReference, SubclassSelector]
#endif
            public HNSFStateDecision[] conditions;
            public AssetRef<Tag> toStateMovesetTag;
            public AssetRef<HNSFState> state;
            public bool overrideTransitionRequests;
            public bool checkToStateCondition = true;
            public bool checkToStateInputCondition;
            public bool setFrame = true;
            [DrawIf(nameof(setFrame), true)]
            public int toFrame = 0;
        }

        public AssetRef<Tag>[] validMovesetsForUsage;
        public StateWithCondition[] states;

        public virtual bool IsMovesetValidForSpecialUsage(AssetRef<Tag> moveset)
        {
            for (int i = 0; i < validMovesetsForUsage.Length; i++)
            {
                if (validMovesetsForUsage[i].Id == moveset.Id) return true;
            }
            return false;
        }
        
        public virtual bool GetBestState(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var savedWorkingState = stateContext.workingState;
            var r = FindBestState(frame, entity, ref stateContext);
            stateContext.workingState = savedWorkingState;
            return r;
        }

        protected virtual bool FindBestState(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            foreach(var state in states)
            {
                stateContext.workingState = state.state;
                bool conditionsValid = true;
                foreach (var deci in state.conditions)
                {
                    if (deci.Decide(frame, entity, ref stateContext)) continue;
                    conditionsValid = false;
                    break;
                }
                if (conditionsValid == false) continue;
                if (!frame.TryFindAsset(state.state.Id, out HNSFState toState)) continue;
                if (!state.overrideTransitionRequests 
                    && stateContext.agentData->toStateRequested) continue;
                if (state.checkToStateCondition
                    && toState.CheckConditions(frame, entity, ref stateContext) == false) continue;

                stateContext.agentData->toStateRequested = true;
                stateContext.agentData->toState = state.state;
                stateContext.agentData->toFrame = state.toFrame;
                return true;
            }
            return false;
        }
    }
}