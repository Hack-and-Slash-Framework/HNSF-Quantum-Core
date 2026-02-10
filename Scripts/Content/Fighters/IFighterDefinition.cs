using Cysharp.Threading.Tasks;
using Quantum;
using HnSF;
using UnityEngine;

public class IFighterDefinition : IContentDefinition
{
    public virtual int Health { get; }

    public virtual UniTask<bool> LoadVisualRepresentation() => new(false);
    public virtual GameObject GetVisualRepresentation() => null;
    public virtual void UnloadVisualRepresentation(){}
    public virtual GameObject GetFighter() => null;
    public virtual BattleActorDefinition GetFighterQuantum() => null;
    public virtual ModAssetSoftReferenceParam[] GetHUDReferences() => null;
    public virtual TaggedModAssetSoftReference[] GetOverrideHUDReferences() => null;
    public virtual BaseCommandListDefinition GetCommandList() => null;
}
