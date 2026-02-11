using Quantum;

namespace HnSF
{
    public unsafe class EntityInputDebugger : QuantumEntityViewComponent
    {
        public QuantumEntityView customEntityView;

        private DispatcherSubscription _updateViewDispatcher;

        public override void OnUpdateView()
        {
            if (PredictedFrame.Unsafe.TryGetPointer(customEntityView.EntityRef, out ActorInputBuffer* ActorInputBuffer))
            {
                
            }
        }
    }
}