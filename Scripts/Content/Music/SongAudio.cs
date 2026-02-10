using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(fileName = "SongAudio", menuName = "HnSF/SongAudio")]
    public class SongAudio : ScriptableObject
    {
        public AudioClip[] audioClips;
        [Range(0.0f, 1.0f)]public float volume = 1.0f;
        [Range(-3.0f, 3.0f)]public float pitch = 1.0f;

        public SongLoopType loopType = SongLoopType.IntroLoop;
        public double introBoundary;
        public double loopingBoundary;
    }
}