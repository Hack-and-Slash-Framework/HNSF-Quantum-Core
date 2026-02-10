using Cysharp.Threading.Tasks;

[System.Serializable]
public abstract class IMapDefinition : IContentDefinition
{
    public abstract Quantum.Map GetMapAsset();
    public abstract string GetSceneName();
    public abstract UniTask<bool> LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode);
    public abstract UniTask UnloadMap();
}
