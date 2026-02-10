using Quantum;
using UnityEngine;

namespace HnSF
{
    public unsafe class EntityPhysicsDebugger : MonoBehaviour
    {
        public QuantumEntityView customEntityView;

        protected DispatcherSubscription _updateViewDispatcher;

        protected virtual void OnEnable()
        {
            _updateViewDispatcher =
                QuantumCallback.Subscribe(this, (CallbackUpdateView callback) => UpdateView(callback));
        }

        protected virtual void OnDisable()
        {
            QuantumCallback.Unsubscribe(_updateViewDispatcher);
        }

        protected virtual void UpdateView(CallbackUpdateView callback)
        {
            var game = callback.Game;

            if (!game.Frames.Predicted.Unsafe.TryGetPointer<CharacterController3D>(customEntityView.EntityRef,
                    out var phs)) return;
        }
    }
}