using Cysharp.Threading.Tasks;

public abstract class BaseContentDefinitionContainer<T> : IContentDefinition where T : IContentDefinition
{
    public abstract UniTask<bool> LoadDefinitions();
    public abstract T[] GetDefinitions();
    public abstract void UnloadDefinitions();
}