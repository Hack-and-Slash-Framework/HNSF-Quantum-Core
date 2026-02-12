using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Song Definition")]
    public partial class UModSongDefinition : BaseSongDefinition, IOnUModPrebuild
    {
        public override string Name => songName;
        public override string Description => description;
        
        [SerializeField] private string songName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private ExternalModAssetSoftReference songAudioReference;
        [SerializeField, HideInInspector] private ModAssetSoftReference songAudioRef;
        
        [NonSerialized] private LoadedAssetHandleWrapper _songAudioHandle;

        public void OnUModPrebuild()
        {
            songAudioRef = songAudioReference ? songAudioReference.reference : default;
        }
        
        public override async UniTask<bool> LoadAssets()
        {
            if (_songAudioHandle.IsValid()) return true;
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            try
            {
                var crefLoadResult = await contentManager.LoadAssetFromModAsync(songAudioRef);
                if (!crefLoadResult.result) throw new Exception($"Failed to load content reference. {songAudioRef.ToString()}");
                _songAudioHandle = crefLoadResult.handle;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading Song {songName} ({name}). {e}");
                return false;
            }
            
            return true;
        }

        public override SongAudio GetSong()
        {
            return _songAudioHandle.umodHandle.Result as SongAudio;
        }

        public override void UnloadAssets()
        {
            _songAudioHandle = default;
        }

        public override void Unload()
        {
        }
    }
}