using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF
{
    [Serializable]
    public class EventReceiverScreenShakeBase
    {
        protected List<IDisposable> _disposableCallbacks = new List<IDisposable>();
        
        public virtual void Initialize()
        {
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventCanceled c) => WhenEventCanceled(c)));
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventConfirmed c) => WhenEventConfirmed(c)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventCauseScreenShake e) => FireScreenShakeImpulse(e)));
        }
        
        public virtual void Teardown()
        {
            for (int i = 0; i < _disposableCallbacks.Count; i++)
            {
                _disposableCallbacks[i].Dispose();
            }

            _disposableCallbacks.Clear();
        }
        
        protected virtual void FireScreenShakeImpulse(EventCauseScreenShake callback)
        {
        }

        protected virtual void WhenEventConfirmed(CallbackEventConfirmed callback)
        {
        }

        protected virtual void WhenEventCanceled(CallbackEventCanceled callback)
        {
        }
    }
}