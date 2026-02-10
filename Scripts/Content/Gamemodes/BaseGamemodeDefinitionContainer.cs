using Cysharp.Threading.Tasks;

public abstract class BaseGamemodeDefinitionContainer : IContentDefinition
{
    public abstract UniTask<bool> LoadGamemodeDefinitions();
    public abstract BaseGamemodeDefinition[] GetGamemodes();
    public abstract void UnloadGamemodeDefinitions();
}