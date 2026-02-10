using HnSF.core.state;
using HnSF.core.state.functions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF
{
    public unsafe class EventActionStateChangeOnHit : HNSFEventAction
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public StateFunctionAssetRef stateReference = new StateFunctionAssetRef();

        public int minimumHitCount = 1;
        
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)
                || !frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return;

            if (boxCombatant->GetTotalHitCount(frame) < minimumHitCount) return;

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