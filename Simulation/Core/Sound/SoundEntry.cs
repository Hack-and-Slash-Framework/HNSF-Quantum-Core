using System;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Localization;
#endif

namespace Quantum
{
    public partial class SoundEntry : AssetObject
    {
        public string label;
        public AssetRef<Tag>[] assignedSoundbanks = Array.Empty<AssetRef<Tag>>();
        public AssetRef<Tag> tag;
#if QUANTUM_UNITY
        public AudioClip clip;
#endif
        public float baseVolume = 1.0f;
        public FP clipLength = 0;
        
#if QUANTUM_UNITY
        public FP subtitleShowTime = 2;
        public LocalizedString subtitleString;
#endif
        
#if QUANTUM_UNITY
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (clip == null) return;
            clipLength = FP.FromFloat_UNSAFE(clip.length);
        }
#endif
    }
}