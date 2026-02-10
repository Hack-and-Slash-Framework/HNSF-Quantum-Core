using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine.Pool;

namespace HnSF
{
    [Serializable]
    public class SubtitledVoiceManager
    {
        private Dictionary<EventKey, (EntitySoundManager, GameAudioSource)> _unconfirmedSounds = new();
        private List<IDisposable> _disposableCallbacks = new List<IDisposable>();
        
        public QuantumEntityViewUpdater viewUpdater;
        public Dictionary<AudioSourceConfig, ObjectPool<GameAudioSource>> audioSourcePools = new();
        
        public virtual void Initialize()
        {
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventPlaySubtitledSoundEntry e) => PlaySoundEntryEvent(e)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventPlaySubtitledSoundbankEntry e) => PlaySoundbankEntryEvent(e)));
        }

        public virtual void Teardown()
        {
            for (int i = 0; i < _disposableCallbacks.Count; i++)
            {
                _disposableCallbacks[i].Dispose();
            }

            _disposableCallbacks.Clear();
        }

        public virtual void Update()
        {
        }

        protected virtual void PlaySoundEntryEvent(EventPlaySubtitledSoundEntry callback)
        {
            
        }
        
        protected virtual void PlaySoundbankEntryEvent(EventPlaySubtitledSoundbankEntry callback)
        {
        }
    }
}