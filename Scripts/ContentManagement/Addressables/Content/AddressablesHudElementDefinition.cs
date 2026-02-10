using System;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Hud Element Definition")]
    public class AddressablesHudElementDefinition : BaseHudElementDefinition
    {
        public override string Name => label;
        public override string Description => description;

        public override AssetRef<Tag> ElementParent => elementParent;

        [SerializeField] private string label;
        [SerializeField, TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> hudElementReference;
        [SerializeField] private AssetRef<Tag> elementParent;
        
        [NonSerialized] private AsyncOperationHandle<GameObject> assetHandle;

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