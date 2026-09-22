using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Command List Entry")]
    public partial class AddressablesCommandListEntry : BaseCommandListEntry
    {
        public override BaseCommandListEntry[] ChildCommands => children;

        public BaseCommandListEntry[] children;

        [SerializeField] private Sprite thumbnail;
        [SerializeField] private AssetReferenceT<VideoClip> demonstrationVideoClip;
        [NonSerialized] private AsyncOperationHandle<VideoClip> videoClipHandle;

        protected override async UniTask LoadAssetsInternal()
        {
            try
            {
                videoClipHandle = Addressables.LoadAssetAsync<VideoClip>(demonstrationVideoClip);
                await videoClipHandle;

                ReportAssetsLoadResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading command list entry's video clip ({name}). {e}");
                ReportAssetsLoadResult(false);
            }
            finally
            {
                loadAssetsCompletionSource = null;
            }
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
            if(videoClipHandle.IsValid())
                Addressables.Release(videoClipHandle);
            videoClipHandle = default;
        }
    }
}