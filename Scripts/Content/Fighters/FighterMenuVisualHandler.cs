using Quantum;
using UnityEngine;

namespace HnSF
{
    public class FighterMenuVisualHandler : MonoBehaviour
    {
        [System.Serializable]
        public class TagToAnimInfo
        {
            public string name;
            public AssetRef<Tag> tag;
            public AnimationClip clip;
        }

        public TagToAnimInfo[] animsByTag;
        public Animation animator;
        
        public void PlayAnimation(AssetRef<Tag> animTag)
        {
            PlayAnimation(GetAnimationFromTag(animTag));
        }

        public void PlayAnimation(AnimationClip clip)
        {
            if (clip == null) return;
            animator.clip = clip;
            animator.Play();
        }

        public AnimationClip GetAnimationFromTag(AssetRef<Tag> animTag)
        {
            foreach (var abt in animsByTag)
            {
                if (abt.tag != animTag) continue;
                return abt.clip;
            }
            return null;
        }
    }
}