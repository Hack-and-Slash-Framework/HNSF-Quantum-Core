using System;
using System.Collections.Generic;
using HnSF;
using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct PlaySoundRequest
    {
        [System.Serializable]
        public struct SoundReference
        {
            public int chance;
            public AssetRef<SoundEntry> soundRef;
            public FP volume;
            public FP minPitch;
            public FP maxPitch;

            public SoundReference Clone()
            {
                return new SoundReference()
                {
                    chance = chance,
                    soundRef = soundRef,
                    volume = volume,
                    minPitch = minPitch,
                    maxPitch = maxPitch,
                };
            }
        }
        
        public bool parentedToSelf;
        public SoundReference[] sounds;
        public WeightedList<int> soundsWeighted;
        public AssetRef<AudioSourceConfig> audioSourceConfig;
        public FP minDistance;
        public FP maxDistance;
        public FPVector3 positionOffset;
        public AssetRef<Tag> tag;
        public bool cancelSameSound;
        public bool cancelSameTag;
        public bool ignoreIfSoundPlaying;
        public bool ignoreIfTagPlaying;
        public bool isGlobal;

        public void OnValidate()
        {
            if (sounds == null)
                sounds = Array.Empty<SoundReference>();
            if(soundsWeighted == null)
                soundsWeighted = new WeightedList<int>();
            soundsWeighted.Clear();
            
            var itemsList = new List<WeightedListItem<int>>();

            for (int i = 0; i < sounds.Length; i++)
            {
                itemsList.Add(new WeightedListItem<int>(i, sounds[i].chance));
            }
            soundsWeighted.Add(itemsList);
        }
        
        public SoundReference GetSound()
        {
            return sounds.Length == 0 ? default : sounds[0];
        }

        public SoundReference GetRngSound(RNGSession* rngSession)
        {
            var index = soundsWeighted.Next(rngSession);
            return index < 0 ? default : sounds[index];
        }
        
        public int GetRngSoundByIndex(RNGSession* rngSession)
        {
            if (sounds.Length == 0) return -1;
            return soundsWeighted.Next(rngSession);
        }

        public PlaySoundRequest Clone()
        {
            var clone = this;
            
            clone.sounds = new SoundReference[sounds.Length];
            for (int i = 0; i < sounds.Length; i++)
            {
                clone.sounds[i] = sounds[i].Clone();
            }
            clone.soundsWeighted = new WeightedList<int>();
            clone.OnValidate();
            return clone;
        }
    }
}