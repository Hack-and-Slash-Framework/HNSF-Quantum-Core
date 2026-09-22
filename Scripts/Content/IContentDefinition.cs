using System;
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

    public virtual bool AreAssetsLoaded => areAssetsLoaded;
    
    [NonSerialized] protected UniTaskCompletionSource<bool> loadAssetsCompletionSource = null;
    [NonSerialized] protected bool areAssetsLoaded = false;
    
    public virtual UniTask<bool> Load(string id)
    {
        _id = id;
        return new UniTask<bool>(true);
    }

    public UniTask<bool> LoadAssets()
    {
        if (areAssetsLoaded)
            return UniTask.FromResult(true);
            
        if (loadAssetsCompletionSource != null)
            return loadAssetsCompletionSource.Task;
        
        loadAssetsCompletionSource = new UniTaskCompletionSource<bool>();
        LoadAssetsInternal().Forget();
        return loadAssetsCompletionSource?.Task ?? UniTask.FromResult(true);
    }

    protected virtual UniTask LoadAssetsInternal()
    {
        loadAssetsCompletionSource.TrySetResult(true);
        loadAssetsCompletionSource = null;
        areAssetsLoaded = true;
        return UniTask.FromResult(true);
    }

    protected virtual void ReportAssetsLoadResult(bool result)
    {
        switch (result)
        {
            case true:
                loadAssetsCompletionSource.TrySetResult(true);
                areAssetsLoaded = true;
                break;
            case false:
                loadAssetsCompletionSource.TrySetResult(false);
                areAssetsLoaded = false;
                UnloadAssets();
                break;
        }
        loadAssetsCompletionSource = null;
    }

    public virtual void UnloadAssets()
    {
        areAssetsLoaded = false;
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
