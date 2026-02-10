using Cysharp.Threading.Tasks;

public abstract class BaseMapDefinitionContainer : IContentDefinition
{
    public abstract UniTask<bool> LoadMapDefinitions();
    public abstract IMapDefinition[] GetMaps();
    public abstract void UnloadMapDefinitions();
}