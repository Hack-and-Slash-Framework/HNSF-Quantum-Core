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
        
        [SerializeField] protected string songName;
        [SerializeField, TextArea] protected string description;
        [SerializeField] protected ExternalModAssetSoftReference songAudioReference;
        [SerializeField, HideInInspector] protected ModAssetSoftReference songAudioRef;
        
        [NonSerialized] protected LoadedAssetHandleWrapper _songAudioHandle;

        public void OnUModPrebuild()
        {
            songAudioRef = songAudioReference ? songAudioReference.reference : default;
        }
        
        protected override async UniTask LoadAssetsInternal()
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            try
            {
                var crefLoadResult = await contentManager.LoadAssetFromModAsync(songAudioRef);
                if (crefLoadResult == null) throw new Exception($"Failed to load content reference. {songAudioRef.ToString()}");
                _songAudioHandle = crefLoadResult;
                
                ReportAssetsLoadResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading Song {songName} ({name})");
                Debug.LogException(e);
                ReportAssetsLoadResult(false);
            }
        }

        public override SongAudio GetSong()
        {
            return _songAudioHandle.GetAsset<SongAudio>();
        }

        public override void UnloadAssets()
        {
            _songAudioHandle?.Release();
            _songAudioHandle = null;
        }

        public override void Unload()
        {
        }
    }
}