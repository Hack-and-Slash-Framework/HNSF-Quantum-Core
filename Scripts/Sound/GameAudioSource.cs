using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public class GameAudioSource : MonoBehaviour
    {
        public SoundEntry soundEntry;
        public AudioSourceConfig config;
        public AudioSource audioSource;
        public EntitySoundManager owner;

        public List<Vector3Int> inSlices;
    }
}