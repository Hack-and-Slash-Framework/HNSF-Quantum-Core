using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF
{
    [Serializable]
    public class MatchScreenShakeManager
    {
        private List<IDisposable> _disposableCallbacks = new List<IDisposable>();
        
        public ImpulseSourceGrouping impulseSourceGrouping;
        public float defaultDistance = 10;
        
        public virtual void Initialize()
        {
            impulseSourceGrouping.Initialize();
            
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
            EventKey eventKey = (EventKey)callback;

            if (callback.shakeFrames <= 0 || callback.shakeStrength == 0) return;

            if (!impulseSourceGrouping.idToImpulseGroup.TryGetValue(callback.shakeType, out var value))
            {
                return;
            }
            
            foreach (var impulseSource in value.impulseSources)
            {
                impulseSource.ImpulseDefinition.ImpulseDuration = ((float)callback.shakeFrames / (float)callback.Game.Frames.Predicted.UpdateRate);
                impulseSource.ImpulseDefinition.DissipationDistance = callback.distance > 0 ? callback.distance.AsFloat : defaultDistance;
                
                var impulseOrigin = callback.origin.ToUnityVector3();

                if (callback.isGlobal)
                {
                    
                }
                
                impulseSource.GenerateImpulseAt(impulseOrigin, impulseSource.DefaultVelocity * callback.shakeStrength.AsFloat);
            }
        }

        protected virtual void WhenEventConfirmed(CallbackEventConfirmed callback)
        {
        }

        protected virtual void WhenEventCanceled(CallbackEventCanceled callback)
        {
        }
    }
}