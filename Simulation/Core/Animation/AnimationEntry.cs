using System.Collections.Generic;
using HnSF;
using UnityEngine.Serialization;
#if QUANTUM_UNITY
using UnityEngine;
#endif
#if ENABLE_ANIMANCER
using Animancer;
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
#if ENABLE_ANIMANCER
            [SerializeReference]
            public ITransition animancerTransition;
#endif
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
        [FormerlySerializedAs("fadeOutTime")] public float defaultFadeOutTime = 0.1f;
        public bool overrideAllFadeIns = false;
        [DrawIf(nameof(overrideAllFadeIns), true)]
        public float overridenFadeInTime = 0;
        public EntityAnimationBlendTable fadeOutDurations = new EntityAnimationBlendTable();
        
        public AnimWithTargetEntry[] animsTargets;
        
        private void OnValidate()
        {
            fadeOutDurations.BuildDictionary();
        }

        public float GetFadeOutDuration(AssetRef<AnimationEntry> toTarget)
        {
            return fadeOutDurations.blends.GetValueOrDefault(toTarget, defaultFadeOutTime);
        }

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
        
#if ENABLE_ANIMANCER
        public ITransition GetAnimancerTransitionForTarget(AssetRef<Tag> targetTag)
        {
            foreach (var v in animsTargets)
            {
                if (v.animTargetTag != targetTag) continue;
                return v.animancerTransition;
            }
            return null;
        }
#endif
    }
}