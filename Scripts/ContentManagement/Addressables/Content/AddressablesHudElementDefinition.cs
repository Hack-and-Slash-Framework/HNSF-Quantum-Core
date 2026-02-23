using System;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
#if HNSF_DISABLE_CONTENT_ASSET_MENU
#else
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Hud Element Definition")]
#endif
    public partial class AddressablesHudElementDefinition : BaseHudElementDefinition
    {
        public override string Name => label;
        public override string Description => description;

        public override AssetRef<Tag> ElementParent => elementParent;

        [SerializeField] protected string label;
        [SerializeField, TextArea] protected string description;
        [SerializeField] protected AssetReferenceT<GameObject> hudElementReference;
        [SerializeField] protected AssetRef<Tag> elementParent;
        
        [NonSerialized] protected AsyncOperationHandle<GameObject> assetHandle;

        public override async UniTask<bool> LoadAssets()
        {
            if (assetHandle.IsValid() && assetHandle.Status == AsyncOperationStatus.Succeeded) return true;
            
            try
            {
                if (!assetHandle.IsValid()) assetHandle = Addressables.LoadAssetAsync<GameObject>(hudElementReference);
                await assetHandle;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading HUD Element {label} ({name}). {e}");
                return false;
            }
            
            return true;
        }

        public override HudElementContainer GetElementContainer()
        {
            return assetHandle.Result?.GetComponent<HudElementContainer>();
        }

        override public GameObject GetElementPrefab()
        {
            return assetHandle.Result?.gameObject;
        }

        public override void UnloadAssets()
        {
            if(assetHandle.IsValid() && assetHandle.Status == AsyncOperationStatus.Succeeded)
                Addressables.Release(assetHandle);
        }

        public override void Unload()
        {
        }
    }
}