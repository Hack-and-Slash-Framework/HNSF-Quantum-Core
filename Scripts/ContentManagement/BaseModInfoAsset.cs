using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using Quantum;
using Object = UnityEngine.Object;

[System.Serializable]
public class BaseModInfoAsset : ScriptableObject
{
    public virtual LoadedModDefinition ModDefinition { get; set; }
    public virtual string ModID { get; }
    public virtual string ModName { get; }
    public virtual string ModVersion { get; }
    public virtual ModOnlineRequirement OnlineRequirement { get; }

    public virtual void Build()
    {
        
    }
    
    /// <summary>
    /// Called right when the mod this ModInfoAsset belongs to loads.
    /// </summary>
    public virtual void OnLoad()
    {
        
    }

    /// <summary>
    /// Called right before the mod this ModInfoAsset belongs to unloads.
    /// </summary>
    public virtual void OnUnload()
    {
        
    }
   
    /// <summary>
    /// Gets a list of all content (loaded or not) in this mod.
    /// </summary>
    /// <returns>A list of the IDs of content.</returns>
    public virtual List<string> GetAssetList()
    {
        return new List<string>();
    }
    
    /// <summary>
    /// Gets a list of all content (loaded or not) in this mod pagniated.
    /// </summary>
    /// <param name="page">The page wanted.</param>
    /// <param name="pageCount">How many asset IDs per page.</param>
    /// <returns>The page's list of IDs.</returns>
    public virtual List<string> GetAssetListPaginated(int page = 0, int pageCount = 100)
    {
        return new List<string>();
    }

    public virtual List<string> GetAssetListByType<T>() where T : UnityEngine.Object
    {
        return new List<string>();
    }

    /// <summary>
    /// Gets a list of all loaded content in this mod.
    /// </summary>
    /// <returns>A list of the IDs of loaded content.</returns>
    public virtual List<string> GetLoadedAssetList()
    {
        return new List<string>();
    }
    
    /// <summary>
    /// Gets a list of all loaded content in the mod with the given type.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    /// <returns>A list of the IDs of the loaded content for the given type.</returns>
    public virtual List<string> GetLoadedAssetListByType<T>() where T : UnityEngine.Object
    {
        return new List<string>();
    }

    public virtual bool HasAsset(string id)
    {
        return false;
    }

    public virtual bool IsAssetLoaded(string id)
    {
        return false;
    }
    
    /// <summary>
    /// Loads an asset by the ID given synchronously.
    /// </summary>
    /// <param name="id">The ID of the asset.</param>
    /// <returns>True if the asset was loaded; otherwise false.</returns>
    public virtual AssetLoadResult LoadAssetByID(string id)
    {
        return default;
    }
    
    /// <summary>
    /// Loads an asset by the ID given synchronously.
    /// </summary>
    /// <param name="id">The ID of the asset.</param>
    /// <returns>True if the asset was loaded; otherwise false.</returns>
    public virtual AssetLoadResult LoadAssetByID<T>(string id) where T : UnityEngine.Object
    {
        return default;
    }
    
    /// <summary>
    /// Loads an asset by the ID given asynchronously.
    /// </summary>
    /// <param name="id">The ID of the asset.</param>
    /// <returns>True if the asset was loaded; otherwise false.</returns>
    public virtual UniTask<AssetLoadResult> LoadAssetByIDAsync(string id)
    {
        return new UniTask<AssetLoadResult>();
    }
    
    /// <summary>
    /// Loads an asset by the ID given asynchronously.
    /// </summary>
    /// <param name="id">The ID of the asset.</param>
    /// <returns>True if the asset was loaded; otherwise false.</returns>
    public virtual UniTask<AssetLoadResult> LoadAssetByIDAsync<T>(string id) where T : UnityEngine.Object
    {
        return new UniTask<AssetLoadResult>();
    }
    
    /// <summary>
    /// Registers all quantum assets to quantum db.
    /// </summary>
    public virtual void RegisterQuantumAssets(QuantumUnityDB quantumUnityDB)
    {
        
    }

    /// <summary>
    /// Deregisters all quantum assets from quantum db.
    /// </summary>
    public virtual void UnregisterQuantumAssets(QuantumUnityDB quantumUnityDB)
    {
        
    }

    public virtual Object GetAssetByID(string id, bool autoLoad = false)
    {
        return null;
    }
    
    public virtual T GetAssetByID<T>(string id, bool autoLoad = false) where T : UnityEngine.Object
    {
        return null;
    }

    public virtual List<T> GetAssetsByType<T>(bool includeInheritors = true) where T : UnityEngine.Object
    {
        return null;
    }

    public virtual System.Object GetAssetInfo(string id)
    {
        return null;
    }

    public virtual void ReleaseAsset(LoadedAssetHandleWrapper assetHandle)
    {
        
    }

    public virtual void ReleaseAll()
    {
        
    }
}
