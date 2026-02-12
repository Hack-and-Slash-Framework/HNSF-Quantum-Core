using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UMod;

namespace HnSF
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Map Definition")]
    public partial class UModMapDefinition : IMapDefinition
    {
        [SerializeField] private string mapName;
        [SerializeField, TextArea] private string mapDescription;
        [SerializeField] private Quantum.Map mapAsset;

        [SerializeField] private string sceneName;

        [NonSerialized] private ModAsyncOperation sceneHandle;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            sceneHandle = default;
            return new UniTask<bool>(true);
        }

        public override Quantum.Map GetMapAsset()
        {
            return mapAsset;
        }

        public override string GetSceneName()
        {
            if (!sceneHandle.IsSuccessful) return null;
            return sceneName + ".copy";
        }

        public override async UniTask<bool> LoadMap(LoadSceneMode loadMode)
        {
            var modHost = (modDefinition as UModLoadedModDefinition).modHost;

            sceneHandle = modHost.Scenes.LoadAsync(sceneName, loadMode == LoadSceneMode.Additive);
            await sceneHandle;
            return sceneHandle.IsSuccessful;
        }

        public override async UniTask UnloadMap()
        {
            await SceneManager.UnloadSceneAsync(sceneName);
        }

        public override void Unload()
        {
            sceneHandle = null;
        }
    }
}