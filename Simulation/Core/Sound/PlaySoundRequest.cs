using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct PlaySoundRequest
    {
        [System.Serializable]
        public struct SoundReference
        {
            public FP chance;
            public AssetRef<SoundEntry> soundRef;
            public FP volume;
            public FP minPitch;
            public FP maxPitch;
        }
        
        public bool parentedToSelf;
        public SoundReference[] sounds;
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
        public FP chance;

        public SoundReference GetSound()
        {
            return sounds.Length == 0 ? default : sounds[0];
        }

        public SoundReference GetRngSound(RNGSession* rngSession)
        {
            var index = GetRngSoundByIndex(rngSession);
            return index < 0 ? default : sounds[index];
        }
        
        public int GetRngSoundByIndex(RNGSession* rngSession)
        {
            if (chance > 0 && chance < 1 && rngSession->NextInclusive() > chance) return -1;

            if (sounds.Length == 1) return sounds.Length - 1;
            
            for (int i = 0; i < sounds.Length; i++)
            {
                if(rngSession->NextInclusive() >= sounds[i].chance)
                    continue;
                return i;
            }
            return sounds.Length-1;
        }
    }
}