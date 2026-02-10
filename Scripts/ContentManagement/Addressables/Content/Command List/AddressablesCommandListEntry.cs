using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Command List Entry")]
    public class AddressablesCommandListEntry : BaseCommandListEntry
    {
        public override BaseCommandListEntry[] ChildCommands => children;

        public BaseCommandListEntry[] children;

        [SerializeField] private Sprite thumbnail;
        [SerializeField] private AssetReferenceT<VideoClip> demonstrationVideoClip;
        [NonSerialized] private AsyncOperationHandle<VideoClip> videoClipHandle;

        public override async UniTask<bool> LoadAssets()
        {
            try
            {
                if (!videoClipHandle.IsValid())
                    videoClipHandle = Addressables.LoadAssetAsync<VideoClip>(demonstrationVideoClip);
                await videoClipHandle;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading command list entry's video clip ({name}). {e}");
                return false;
            }
            
            return true;
        }

        public override Sprite GetImage()
        {
            return thumbnail;
        }

        public override VideoClip GetVideo()
        {
            return videoClipHandle.Result;
        }
        
        public override void UnloadAssets()
        {
            if(videoClipHandle.IsValid() && videoClipHandle.Status == AsyncOperationStatus.Succeeded)
                Addressables.Release(videoClipHandle);
        }
    }
}