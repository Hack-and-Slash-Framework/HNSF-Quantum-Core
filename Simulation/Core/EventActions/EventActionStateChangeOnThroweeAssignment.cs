using HnSF.core.state;
using HnSF.core.state.functions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF
{
    public unsafe class EventActionStateChangeOnThroweeAssignment : HNSFEventAction
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public StateFunctionAssetRef stateReference = new StateFunctionAssetRef();

        //public int[] validThrowees = Array.Empty<int>();
        
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)
                || !frame.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return;
            
            var throwees = frame.ResolveDictionary(isThrowing->throwees);
            if (throwees.Count == 0) return;
            
            var sc = new HNSFStateContext(frame, entity);
            var stateAssetRef = stateReference.Execute(frame, entity, ref sc);
            if (!stateAssetRef.IsValid) return;
            
            gsm->stateAgent.stateData.toStateRequested = true;
            gsm->stateAgent.stateData.toState = new AssetRef<HNSFState>(stateAssetRef);
            gsm->stateAgent.stateData.toFrame = 0;
            
            HNSFStateHelper.Generic.CheckForStateChange(frame, entity, gsm, sc.aiConfig);
        }
    }
}