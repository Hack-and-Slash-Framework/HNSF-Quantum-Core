using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF
{
    public class MusicLooper : MonoBehaviour
    {
        [SerializeField] private MusicLooperTrack trackPrefab;
        public List<MusicLooperTrack> tracks = new List<MusicLooperTrack>();
        [SerializeField] private AudioSource referenceAudioSource;

        private double nextEventTime;

        public SongAudio song;

        private float currentVolume;

        public void Play(SongAudio wantedSong, float volume = 1.0f)
        {
            this.song = wantedSong;
            ClearTracks();
            currentVolume = wantedSong.volume;
            referenceAudioSource.volume = wantedSong.volume * volume;
            referenceAudioSource.pitch = wantedSong.pitch;
            for (int i = 0; i < song.audioClips.Length; i++)
            {
                var trackObj = new GameObject($"Track{i+1}").AddComponent<MusicLooperTrack>();
                trackObj.transform.SetParent(transform);
                tracks.Add(trackObj);
                trackObj.Play(referenceAudioSource, song.audioClips[i], song.loopType, 
                    song.introBoundary, song.loopingBoundary);
                if(i > 0) trackObj.SetVolume(0);
            }
        }

        public void SetTrackVolume(int index, float volume)
        {
            tracks[index].SetVolume(currentVolume * volume);
        }

        public void SetTrackVolumeSlice(int index, float volume)
        {
            if (tracks.Count == 0) return;
            if (tracks.Count == 1)
            {
                tracks[0].SetVolume(currentVolume * volume);
                return;
            }
            index = Mathf.Clamp(index, 0, tracks.Count);
            
            float otherTrackSlice = (1.0f - volume) / (tracks.Count-1);

            for (int i = 0; i < tracks.Count; i++)
            {
                if (i == index)
                {
                    tracks[i].SetVolume(currentVolume * volume);
                    continue;
                }
                tracks[i].SetVolume(currentVolume * otherTrackSlice);
            }
        }

        private void ClearTracks()
        {
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                Destroy(tracks[i].gameObject);
                tracks.RemoveAt(i);
            }
        }

        public void Pause()
        {
            
        }
        
        public void Stop()
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].Stop();
            }
        }

        public async UniTask FadeOut(float fadeTime = 0.5f)
        {
            float currentTime = 0;
            float start = tracks[0].GetVolume();
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                foreach (var t in tracks)
                {
                    t.SetVolume(Mathf.Lerp(start, 0.0f, currentTime / fadeTime));
                }
                await UniTask.NextFrame();
            }
        }
    }
}