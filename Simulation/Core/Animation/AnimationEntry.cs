#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class AnimationEntry : AssetObject
    {
        public string entryName;
        public AssetRef<Tag> sharedAnimationTag;

        public enum MixerType
        {
            none,
            Linear,
            Cartesian,
            Directional
        }

        [System.Serializable]
        public partial class AnimWithTargetEntry
        {
            public AssetRef<Tag> animTargetTag;
            public AnimEntry[] anims;
        }

        [System.Serializable]
        public partial class AnimEntry
        {
#if QUANTUM_UNITY
            public AnimationClip clip;
            public Vector2 param;
#endif
        }

        public MixerType mixer;
        public float mixerSmoothing = 0;
        public float playRate = 1;
        public float fadeOutTime = 0.1f;
        public float maxFadeInTime = 0.05f;

        public AnimWithTargetEntry[] animsTargets;

        public bool HasTarget(AssetRef<Tag> targetTag)
        {
            if (animsTargets == null || animsTargets.Length == 0) return false;

            foreach (var v in animsTargets)
            {
                if (v.animTargetTag == targetTag) return true;
            }

            return false;
        }

        public AnimEntry[] GetAnimsForTarget(AssetRef<Tag> targetTag)
        {
            foreach (var v in animsTargets)
            {
                if (v.animTargetTag != targetTag) continue;
                return v.anims;
            }

            return null;
        }
    }
}