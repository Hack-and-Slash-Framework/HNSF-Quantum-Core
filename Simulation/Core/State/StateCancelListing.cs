using System;
using System.Collections.Generic;
using HnSF.core.state;
using HnSF.core.state.decisions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe class StateCancelListing : AssetObject
    {
        [System.Serializable]
        public enum StateCancelListEntryType
        {
            StateByTag,
            StateByAsset
        }
        
        [System.Serializable]
        public class StateCancelListEntry
        {
            public string label;
            public StateCancelListEntryType entryType;
            [DrawIf(nameof(entryType), (int)StateCancelListEntryType.StateByAsset)]
            public AssetRef<HNSFState> stateAssetRef;
            [DrawIf(nameof(entryType), (int)StateCancelListEntryType.StateByTag)]
            public AssetRef<Tag> stateTagAssetRef;
            public bool transitionToSameFrame;
        }

        public string label;
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();
        
        public List<StateCancelListEntry> entries = new List<StateCancelListEntry>();
        
        public int AttemptCancel(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var savedWorkingState = stateContext.workingState;
            
            foreach (var d in conditions)
            {
                if (d.Decide(frame, entity, ref stateContext) == false)
                    return -1;
            }
            
            for (int i = 0; i < entries.Count; i++)
            {
                HNSFState stateAsset = null;

                switch (entries[i].entryType)
                {
                    case StateCancelListEntryType.StateByAsset:
                        stateAsset = frame.FindAsset(entries[i].stateAssetRef);
                        break;
                    case StateCancelListEntryType.StateByTag:
                        if (frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm)
                            && frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet))
                        {
                            stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset,
                                entries[i].stateTagAssetRef, out var stateAssetRef);
                            stateAsset = frame.FindAsset(stateAssetRef);
                        }
                        break;
                }
                
                if(stateAsset == null)
                    continue;
                
                stateContext.workingState = entries[i].stateAssetRef;
                
                if (stateContext.agentData->toStateRequested) continue;
                if (stateAsset.CheckConditions(frame, entity, ref stateContext) == false) continue;
                        
                if (stateAsset.defaultInputConditions.Length > 0 
                    && frame.Unsafe.TryGetPointer<ActorInputBuffer>(entity, out var actorInputInfo) )
                {
                    var gotValidInput = false;
                    foreach (var ic in stateAsset.defaultInputConditions)
                    {
                        if(frame.TryFindAsset(ic, out var inputConditionListAsset) == false
                           || InputHelper.CheckInputConditions(frame, actorInputInfo, inputConditionListAsset.conditions, actorInputInfo->bufferPosition) == -1) continue;
                        gotValidInput = true;
                        break;
                    }
                    if (gotValidInput == false) continue;
                }
                        
                stateContext.agentData->toStateRequested = true;
                stateContext.agentData->toState = entries[i].stateAssetRef;
                stateContext.agentData->toFrame = entries[i].transitionToSameFrame ? stateContext.agentData->frame : 0;
                
                // Success, cleanup.
                stateContext.workingState = savedWorkingState;

                return i;
            }
            return -1;
        }
    }
}
