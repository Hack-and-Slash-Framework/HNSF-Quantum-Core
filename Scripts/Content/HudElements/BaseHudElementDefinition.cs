using Cysharp.Threading.Tasks;
using Quantum;
using HnSF;
using UnityEngine;

[System.Serializable]
public abstract partial class BaseHudElementDefinition : IContentDefinition
{
    public virtual AssetRef<Tag> ElementParent { get; }
    
    public abstract HudElementContainer GetElementContainer();
    public abstract GameObject GetElementPrefab();
}