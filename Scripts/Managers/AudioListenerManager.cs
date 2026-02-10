using System;
using UnityEngine;

namespace HnSF
{
    public class AudioListenerManager : MonoBehaviour
    {
        [SerializeField] private Transform audioListener;
        public Transform AudioListener => audioListener;
        public AudioSource uiSoundAudioSource;

        [NonSerialized] public int lastSetPriority = -100;

        private void LateUpdate()
        {
            lastSetPriority = int.MinValue;
        }

        public void AttemptSetPosition(Vector3 position, int priority = 0)
        {
            if (priority < lastSetPriority) return;
            audioListener.position = position;
            lastSetPriority = priority;
        }
        
        public void AttemptSetRotation(Quaternion rotation, int priority = 0)
        {
            if (priority < lastSetPriority) return;
            audioListener.rotation = rotation;
            lastSetPriority = priority;
        }
        
        public void AttemptSetRotation(Vector3 rotation, int priority = 0)
        {
            if (priority < lastSetPriority) return;
            audioListener.eulerAngles = rotation;
            lastSetPriority = priority;
        }
        
        public void PlayUISound(AudioClip clip, float volume = 1.0f)
        {
            uiSoundAudioSource.PlayOneShot(clip, volume);
        }
    }
}