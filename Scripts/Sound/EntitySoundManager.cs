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
        public Dictionary<GameAudioSource, Vector3> parentedAudioSources = new();
        
        private Dictionary<SoundEntry, List<GameAudioSource>> currentlyPlayingSoundTypes = new();
        public Dictionary<AssetRef<Tag>, List<GameAudioSource>> currentlyPlayingSounds = new();

        [NonSerialized] public Dictionary<AudioSourceConfig, ObjectPool<GameAudioSource>> audioPool = null;
        [NonSerialized] public EntitySoundManager parentManager;

        private void Awake()
        {
            if (EntityView == null)
                return;
            
            EntityView.OnEntityInstantiated.AddListener(WhenEntityInstantiated);
            EntityView.OnEntityDestroyed.AddListener(WhenEntityDestroyed);
        }
        
        protected virtual void WhenEntityInstantiated(QuantumGame arg0)
        {
            
        }

        protected virtual void WhenEntityDestroyed(QuantumGame arg0)
        {
            parentManager?.InheritSoundsFrom(this);
        }

        public override void OnUpdateView()
        {
            foreach (var cpList in currentlyPlayingSounds)
            {
                for (int i = cpList.Value.Count - 1; i >= 0; i--)
                {
                    if (cpList.Value[i].audioSource.isPlaying == false) StopSound(cpList.Value[i]);
                }
            }

            foreach (var parentedSource in parentedAudioSources)
            {
                
            }
        }

        public virtual void InheritSoundsFrom(EntitySoundManager otherSoundManager)
        {
            foreach (var soundList in otherSoundManager.currentlyPlayingSounds)
            {
                var list = soundList.Value;

                foreach (var aSource in soundList.Value)
                {
                    if(aSource == null)
                        continue;
                    
                    RegisterSound(aSource);
                }
            }
        }

        public virtual void RegisterSound(GameAudioSource audioSource)
        {
            var qTag = audioSource.soundEntry.tag;
            if (!currentlyPlayingSounds.ContainsKey(qTag))
                currentlyPlayingSounds.Add(qTag, new List<GameAudioSource>());

            if (currentlyPlayingSounds[qTag].Contains(audioSource)) return;
            
            currentlyPlayingSounds[qTag].Add(audioSource);
            
            audioSourceToTag.Add(audioSource, qTag);
            audioSourceToSoundEntry.Add(audioSource, audioSource.soundEntry);
            currentlyPlayingSoundTypes[audioSource.soundEntry].Add(audioSource);
        }
        
        public virtual bool PlaySound(GameAudioSource audioSource, SoundEntry soundEntry, GameObject parent, Vector3 position,
            float time, float volume, float pitch, AssetRef<Tag> qTag,
            AudioSourceConfig audioSourceConfig, EventKey key, bool stopOtherInstances = false,
            bool stopOthersOfSameTag = false,
            bool ignoreIfSoundPlaying = false, bool ignoreIfTagPlaying = false)
        {
            if(parent) parentedAudioSources.Add(audioSource, audioSource.gameObject.transform.position - transform.position);
            
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

            audioSource.audioSource.Stop();
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

        public virtual void StopSound(GameAudioSource audioSource, bool release = true)
        {
            if (!audioSourceToTag.TryGetValue(audioSource, out var asTag)
                || !audioSourceToSoundEntry.TryGetValue(audioSource, out var soundEntry)) return;
            
            parentedAudioSources.Remove(audioSource);
            
            currentlyPlayingSounds[asTag].Remove(audioSource);
            currentlyPlayingSoundTypes[soundEntry].Remove(audioSource);

            audioSourceToTag.Remove(audioSource);
            audioSourceToSoundEntry.Remove(audioSource);

            audioSource.audioSource.Stop();

            if (!release)
                return;
            
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