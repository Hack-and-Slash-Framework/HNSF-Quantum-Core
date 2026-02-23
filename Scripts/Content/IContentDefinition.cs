using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract partial class IContentDefinition : ScriptableObject
{
    public virtual LoadedModDefinition modDefinition
    {
        get => _modDefinition;
        set => _modDefinition = value;
    }

    public virtual string ID
    {
        get => _id;
        set => _id = value;
    }

    public virtual string Name { get; }
    public virtual string Description { get; }
    public virtual bool Selectable { get; }
    public virtual List<string> Tags => tags;
    [SerializeField] protected List<string> tags;
    protected LoadedModDefinition _modDefinition;
    protected string _id;

    public virtual UniTask<bool> Load(string id)
    {
        _id = id;
        return new UniTask<bool>(true);
    }

    public virtual UniTask<bool> LoadAssets()
    {
        return new UniTask<bool>(true);
    }

    public virtual void UnloadAssets()
    {
        
    }

    public virtual void Unload()
    {
        UnloadAssets();
    }

    public ModAssetSoftReference GetAssetSoftReference()
    {
        return new ModAssetSoftReference()
        {
            mod = modDefinition.modAsset.ModID,
            assetID = ID,
            isFolder = false
        };
    }
}
