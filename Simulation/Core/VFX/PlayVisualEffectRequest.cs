using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct PlayVisualEffectRequest
    {
        [System.Serializable]
        public struct VFXReference
        {
            public FP chance;
            public AssetRef<VisualEffectEntry> vfxReference;
        }
        
        public VFXReference[] visualEffects;
        public FP chance;

        public bool parentedToSelf;
        public AssetRef<Tag> parentBoneTag;
        public bool pauseDuringHitstop;
        public bool positionAsOffset;
        public FPVector3 positionOffset;
        public bool rotationAsOffset;
        public FP rotationOffset;
        public bool rotateToMoveForce;
        public FP forwardOffset;
        
        public VFXReference GetVFX()
        {
            return visualEffects.Length == 0 ? default : visualEffects[0];
        }

        public VFXReference GetRngVFX(RNGSession* rngSession)
        {
            var index = GetRngVFXByIndex(rngSession);
            return index < 0 ? default : visualEffects[index];
        }
        
        public int GetRngVFXByIndex(RNGSession* rngSession)
        {
            if (chance > 0 && chance < 1 && rngSession->NextInclusive() > chance) return -1;

            if (visualEffects.Length == 1) return visualEffects.Length - 1;
            
            for (int i = 0; i < visualEffects.Length; i++)
            {
                if(rngSession->NextInclusive() >= visualEffects[i].chance)
                    continue;
                return i;
            }
            return visualEffects.Length-1;
        }
    }
}