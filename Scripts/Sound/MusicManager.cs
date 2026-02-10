using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public class MusicManager : MonoBehaviour
    {
        public MusicLooper musicLooperPrefab;

        public List<MusicLooper> currentlyPlaying = new List<MusicLooper>();
        public List<MusicLooper> currentlyFading = new List<MusicLooper>();

        public bool disableMusic;
        
        public virtual void Initialize()
        {
            
        }
        
        public SongAudio GetCurrentlyPlayingSong()
        {
            if (currentlyPlaying.Count == 0) return null;
            return currentlyPlaying[0].song;
        }
        
        public MusicLooper Play(SongAudio song, float volume = 1.0f, bool ignoreIfAlreadyPlaying = true)
        {
            if (song == null || disableMusic) return null;
            if (ignoreIfAlreadyPlaying && currentlyPlaying.Count > 0 && currentlyPlaying[0].song == song)
                return currentlyPlaying[0];
            var musicLooper = GameObject.Instantiate(musicLooperPrefab, transform, false);
            currentlyPlaying.Add(musicLooper);
            musicLooper.Play(song, volume);
            return musicLooper;
        }

        public MusicLooper GetLooperForSong(SongAudio song)
        {
            foreach (var ml in currentlyPlaying)
            {
                if (ml.song != song) continue;
                return ml;
            }
            return null;
        }

        public void FadeAll(float timeToFade = 0.5f)
        {
            for (int i = currentlyPlaying.Count-1; i >= 0; i--)
            {
                Fade(i, timeToFade);
            }
        }
        
        public void Fade(int index, float timeToFade = 0.5f)
        {
            _ = currentlyPlaying[index].FadeOut(timeToFade);
            currentlyFading.Add(currentlyPlaying[index]);
            currentlyPlaying.RemoveAt(index);
        }

        public void StopCurrentlyPlaying(bool destroyPlayers = false)
        {
            for (int i = currentlyPlaying.Count-1; i >= 0; i--)
            {
                currentlyPlaying[i].Stop();
                if (destroyPlayers)
                {
                    Destroy(currentlyPlaying[i].gameObject);
                    currentlyPlaying.RemoveAt(i);
                }
            }
        }

        public void StopCurrentlyFading(bool destroyPlayers = false)
        {
            for (int i = currentlyFading.Count-1; i >= 0; i--)
            {
                currentlyPlaying[i].Stop();
                if (destroyPlayers)
                {
                    Destroy(currentlyPlaying[i].gameObject);
                    currentlyPlaying.RemoveAt(i);
                }
            }
        }
    }
}