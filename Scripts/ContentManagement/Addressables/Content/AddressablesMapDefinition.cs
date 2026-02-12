using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace HnSF
{
    [System.Serializable]
#if HNSF_DISABLE_CONTENT_ASSET_MENU
#else
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Map Definition")]
#endif
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: null, sourceClassName: "AddressablesMapDefinition")]
    public partial class AddressablesMapDefinition : IMapDefinition
    {
        public override string Name => _mapName;
        public override string Description => _mapDescription;
        public override bool Selectable => selectable;

        [SerializeField] public bool selectable = true;
        [SerializeField] private string _mapName;
        [SerializeField, TextArea] private string _mapDescription;
        [SerializeField] private AssetReference _sceneReference;
        [SerializeField] private Quantum.Map mapAsset;

        [NonSerialized] private AsyncOperationHandle<SceneInstance> sceneHandle;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            sceneHandle = new AsyncOperationHandle<SceneInstance>();
            return new UniTask<bool>(true);
        }

        public override UniTask<bool> LoadAssets()
        {
            return new UniTask<bool>(true);
        }

        public override Quantum.Map GetMapAsset()
        {
            return mapAsset;
        }

        public override string GetSceneName()
        {
            if (!sceneHandle.IsValid() || sceneHandle.Status != AsyncOperationStatus.Succeeded) return null;
            return sceneHandle.Result.Scene.name;
        }

        public override async UniTask<bool> LoadMap(LoadSceneMode loadMode)
        {
            sceneHandle = Addressables.LoadSceneAsync(_sceneReference, loadMode);
            await sceneHandle;
            return sceneHandle.Status == AsyncOperationStatus.Succeeded;
        }

        public override async UniTask UnloadMap()
        {
            if (!sceneHandle.IsValid() || sceneHandle.Status != AsyncOperationStatus.Succeeded) return;
            await Addressables.UnloadSceneAsync(sceneHandle);
        }

        public override void Unload()
        {
            if (!sceneHandle.IsValid() || sceneHandle.Status != AsyncOperationStatus.Succeeded) return;
            Addressables.Release(sceneHandle);
        }
    }
}