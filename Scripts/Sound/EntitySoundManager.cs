using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Pool;

namespace HnSF
{
    public class EntitySoundManager : QuantumEntityViewComponent
    {
        private Dictionary<GameAudioSource, AssetRef<Tag>> audioSourceToTag = new();
        private Dictionary<GameAudioSource, SoundEntry> audioSourceToSoundEntry = new();

        private Dictionary<SoundEntry, List<GameAudioSource>> currentlyPlayingSoundTypes = new();
        public Dictionary<AssetRef<Tag>, List<GameAudioSource>> currentlyPlayingSounds = new();

        [NonSerialized] public Dictionary<AudioSourceConfig, ObjectPool<GameAudioSource>> audioPool = null;

        public override void OnUpdateView()
        {
            foreach (var cpList in currentlyPlayingSounds)
            {
                for (int i = cpList.Value.Count - 1; i >= 0; i--)
                {
                    if (cpList.Value[i].audioSource.isPlaying == false) StopSound(cpList.Value[i]);
                }
            }
        }

        public virtual bool PlaySound(GameAudioSource audioSource, SoundEntry soundEntry, GameObject parent, Vector3 position,
            float time, float volume, float pitch, AssetRef<Tag> qTag,
            AudioSourceConfig audioSourceConfig, EventKey key, bool stopOtherInstances = false,
            bool stopOthersOfSameTag = false,
            bool ignoreIfSoundPlaying = false, bool ignoreIfTagPlaying = false)
        {
            if (parent) audioSource.transform.SetParent(parent.transform);

            //if (stopOthersOfSameTag && currentlyPlayingSounds.ContainsKey(tag)) 
            if (!currentlyPlayingSounds.ContainsKey(qTag))
                currentlyPlayingSounds.Add(qTag, new List<GameAudioSource>());
            currentlyPlayingSounds[qTag].Add(audioSource);

            if (ignoreIfSoundPlaying && currentlyPlayingSoundTypes.ContainsKey(soundEntry) &&
                currentlyPlayingSoundTypes[soundEntry].Count > 0) return false;
            if (currentlyPlayingSoundTypes.ContainsKey(soundEntry)
                && stopOtherInstances) StopAllInstances(soundEntry);
            if (!currentlyPlayingSoundTypes.ContainsKey(soundEntry))
                currentlyPlayingSoundTypes.Add(soundEntry, new List<GameAudioSource>());

            audioSource.audioSource.clip = soundEntry.clip;
            audioSource.audioSource.volume = soundEntry.baseVolume * volume;
            audioSource.audioSource.time = time;
            audioSource.audioSource.pitch = pitch;
            audioSource.audioSource.Play();

            audioSourceToTag.Add(audioSource, qTag);
            audioSourceToSoundEntry.Add(audioSource, soundEntry);
            currentlyPlayingSoundTypes[soundEntry].Add(audioSource);
            return true;
        }

        protected virtual void StopAllInstances(SoundEntry soundEntry)
        {
            foreach (var aSource in currentlyPlayingSoundTypes[soundEntry])
            {
                var asTag = audioSourceToTag[aSource];
                var asSoundEntry = audioSourceToSoundEntry[aSource];

                currentlyPlayingSounds[asTag].Remove(aSource);

                audioSourceToTag.Remove(aSource);
                audioSourceToSoundEntry.Remove(aSource);

                aSource.audioSource.Stop();

                if (audioPool != null && audioPool.ContainsKey(aSource.config))
                {
                    audioPool[aSource.config].Release(aSource);
                }
                else
                {
                    GameObject.Destroy(aSource.gameObject);
                }
            }

            currentlyPlayingSoundTypes[soundEntry].Clear();
        }

        public virtual void StopSound(GameAudioSource audioSource)
        {
            if (!audioSourceToTag.TryGetValue(audioSource, out var asTag)
                || !audioSourceToSoundEntry.TryGetValue(audioSource, out var soundEntry)) return;

            currentlyPlayingSounds[asTag].Remove(audioSource);
            currentlyPlayingSoundTypes[soundEntry].Remove(audioSource);

            audioSourceToTag.Remove(audioSource);
            audioSourceToSoundEntry.Remove(audioSource);

            audioSource.audioSource.Stop();

            if (audioPool != null && audioPool.ContainsKey(audioSource.config))
            {
                audioPool[audioSource.config].Release(audioSource);
            }
            else
            {
                GameObject.Destroy(audioSource.gameObject);
            }
        }
    }
}