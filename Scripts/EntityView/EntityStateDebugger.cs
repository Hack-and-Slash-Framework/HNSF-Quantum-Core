using System.Collections.Generic;
using HnSF.core.state;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public unsafe class EntityStateDebugger : MonoBehaviour
    {
        public QuantumEntityView customEntityView;
        public List<HNSFState> latestStates = new List<HNSFState>();

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

            var sAgent = game.Frames.Predicted.Unsafe.GetPointer<GenericStateMachine>(customEntityView.EntityRef);
            var ass = game.Frames.Predicted.FindAsset<HNSFState>(sAgent->stateAgent.stateData.state.Id);
            if (!ass) return;

            if (latestStates.Count == 0)
            {
                latestStates.Add(ass);
                return;
            }

            if (latestStates.Count < 10)
            {
                if (ass != latestStates[0]) latestStates.Insert(0, ass);
                return;
            }

            if (latestStates[0] == ass) return;
            latestStates.Insert(0, ass);
            latestStates.RemoveAt(latestStates.Count - 1);
        }
    }
}