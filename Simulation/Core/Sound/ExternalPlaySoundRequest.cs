using System;
using HnSF;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public class ExternalPlaySoundRequest : AssetObject
    {
        public PlaySoundRequest request;

        public ExternalPlaySoundRequest()
        {
            request.soundsWeighted = new WeightedList<int>();
            request.sounds = Array.Empty<PlaySoundRequest.SoundReference>();
            request.minDistance = 0;
            request.maxDistance = 5;
        }

        private void OnValidate()
        {
#if QUANTUM_UNITY
            //if (Application.isPlaying) return;
            request.OnValidate();
#endif
        }
    }
}
