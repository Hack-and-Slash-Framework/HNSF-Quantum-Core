using System;
using System.Collections.Generic;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CheckCancelListing : HNSFStateAction
    {
        [Serializable]
        public class CancelListingEntry
        {
            public string name;
#if QUANTUM_UNITY
            [SerializeReference, SubclassSelector]
#endif
            public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();
            public AssetRef<StateCancelListing> stateListing;
        }

        public CancelListingEntry[] cancelListings = Array.Empty<CancelListingEntry>();
        public bool immediateTransition;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            
            var didTransition = DoAction(frame, targetEntityRef, ref targetStateContext);
            if (actionTargetContext.targetType == StateActionTargetType.Self)
            {
                stateContext = targetStateContext;
                if (immediateTransition) return true;
            }
            return false;
        }

        private bool DoAction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)
                || !frame.TryFindAsset(csm->stateAgent.stateData.state, out var currentState)) return false;
            
            for (int i = 0; i < cancelListings.Length; i++)
            {
                if(!frame.TryFindAsset(cancelListings[i].stateListing, out var cancelListing))
                    continue;

                var conditionsPassed = true;
                foreach (var d in cancelListings[i].conditions)
                {
                    if (d.Decide(frame, entity, ref stateContext) == false)
                    {
                        conditionsPassed = false;
                        break;
                    }
                }
                
                if(!conditionsPassed)
                    continue;

                var stateIndex = cancelListing.AttemptCancel(frame, entity, ref stateContext);
                
                if(stateIndex == -1)
                    continue;
                
                if (immediateTransition)
                {
                    HNSFStateHelper.Generic.CheckForStateChange(frame, entity, csm, stateContext.aiConfig);
                }
                // ...
                break;
            }
            
            return false;
        }
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new CheckCancelListing());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CheckCancelListing;
            if (cancelListings == null) cancelListings = Array.Empty<CancelListingEntry>();
            t.cancelListings = new CancelListingEntry[cancelListings.Length];
            for (int i = 0; i < t.cancelListings.Length; i++)
            {
                t.cancelListings[i].name =  cancelListings[i].name;
                t.cancelListings[i].conditions = new HNSFStateDecision[cancelListings[i].conditions.Length];
                for (int w = 0; w < cancelListings[i].conditions.Length; w++)
                {
                    t.cancelListings[i].conditions[w] = cancelListings[i].conditions[w].Copy();
                }
                t.cancelListings[i].stateListing = cancelListings[i].stateListing;
            }
            return base.CopyTo(target);
        }
    }
}
