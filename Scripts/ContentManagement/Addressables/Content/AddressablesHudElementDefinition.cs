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

        protected override async UniTask LoadAssetsInternal()
        {
            try
            {
                assetHandle = Addressables.LoadAssetAsync<GameObject>(hudElementReference);
                await assetHandle;
                
                ReportAssetsLoadResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading HUD Element {label} ({name})");
                Debug.LogException(e);
                ReportAssetsLoadResult(false);
            }
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
            if(assetHandle.IsValid())
                Addressables.Release(assetHandle);
            assetHandle = default;
        }
    }
}