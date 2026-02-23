using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
#if HNSF_DISABLE_CONTENT_ASSET_MENU
#else
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Song Definition")]
#endif
    public partial class AddressablesSongDefinition : BaseSongDefinition
    {
        public override string Name => songName;
        public override string Description => description;
        
        [SerializeField] protected string songName;
        [SerializeField, TextArea] protected string description;
        [SerializeField] protected AssetReferenceT<SongAudio> songAudioReference;
        
        [NonSerialized] protected AsyncOperationHandle<SongAudio> songAudioHandle;

        public override async UniTask<bool> LoadAssets()
        {
            try
            {
                if (!songAudioHandle.IsValid())
                    songAudioHandle = Addressables.LoadAssetAsync<SongAudio>(songAudioReference);
                await songAudioHandle;
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
            return songAudioHandle.Result;
        }

        public override void UnloadAssets()
        {
            if(songAudioHandle.IsValid() && songAudioHandle.Status == AsyncOperationStatus.Succeeded)
                Addressables.Release(songAudioHandle);
        }

        public override void Unload()
        {
        }
    }
}