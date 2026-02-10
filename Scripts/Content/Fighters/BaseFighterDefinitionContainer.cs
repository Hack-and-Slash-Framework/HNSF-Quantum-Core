using Cysharp.Threading.Tasks;

public abstract class BaseFighterDefinitionContainer : IContentDefinition
{
    public abstract IFighterDefinition[] GetFighters();
}
