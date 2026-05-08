using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HnSF;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "HnSF/Mod Definitions/Addressables ModInfoAsset")]
[System.Serializable]
public class AddressablesModInfoAsset : BaseModInfoAsset
{
    [System.Serializable]
    public class SavedGuidMapping
    {
        public string address;
        public Type assetType;
        public long quantumGuid;
    }
    
    public override string ModID => modID;
    public override string ModName => modName;
    public override string ModVersion => modVersion;
    public override ModOnlineRequirement OnlineRequirement => onlineRequirement;
    public bool ShouldRegisterQuantumAssets { get; set; } = true;
    
    [SerializeField, ReadOnly] public string modGuid;
    [SerializeField] private string modID;
    [SerializeField] public string modAuthor;
    [SerializeField] private string modName;
    [SerializeField] private string modVersion;
    [SerializeField] private ModOnlineRequirement onlineRequirement;
    [SerializeField, HideInInspector] public List<SavedGuidMapping> savedGuidMappings = new List<SavedGuidMapping>();
    
    // Content that has been loaded, sorted by their type.
    [NonSerialized] private Dictionary<Type, List<string>> loadedAssetsByType = new();
    // Content that has been loaded, indexed by their ID.
    [NonSerialized] private Dictionary<string, List<AsyncOperationHandle>> loadedAssetList = new();
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isEditor || Application.isPlaying) return;
        modGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
#endif
    }

    public void SetInfo(string ModIdentifier, string modName, string modVersion, ModOnlineRequirement onlineRequirement)
    {
        this.modID = ModIdentifier;
        this.modName = modName;
        this.modVersion = modVersion;
        this.onlineRequirement = onlineRequirement;
    }
    
    public override void OnLoad()
    {
        Debug.Log($"{ModID} mod (Addressables) loaded.");
    }

    public override void RegisterQuantumAssets(QuantumUnityDB quantumUnityDB)
    {
        if (ModDefinition is AddressablesLocalLoadedModDefinition) return;

        foreach (var guidMapping in savedGuidMappings)
        {
            var source = new QuantumAssetObjectSourceAddressable(guidMapping.address, guidMapping.assetType);
            quantumUnityDB.AddSource(source, new AssetGuid(guidMapping.quantumGuid));
        }
    }

    public override void UnregisterQuantumAssets(QuantumUnityDB quantumUnityDB)
    {
        if (ModDefinition is AddressablesLocalLoadedModDefinition) return;

        foreach (var guidMapping in savedGuidMappings)
        {
            quantumUnityDB.RemoveSource(new AssetGuid(guidMapping.quantumGuid));
        }
    }

    public override List<string> GetAssetList()
    {
        HashSet<string> list = new();
        
        var amd = ModDefinition as AddressablesLoadedModDefinition;

        foreach (var k in amd.resourceLocator.Keys)
        {
            if (amd.resourceLocator.Locate(k, typeof(UnityEngine.Object), out var locs))
            {
                foreach (var l in locs)
                {
                    list.Add(l.PrimaryKey);
                    //list.Add($"{l.PrimaryKey} ({l.ResourceType.Name}) ({l.InternalId})");
                }
            }
        }

        return list.ToList();
    }
    
    public override List<string> GetAssetListByType<T>()
    {
        HashSet<string> list = new();
        
        var amd = ModDefinition as AddressablesLoadedModDefinition;
        
        foreach (var k in amd.resourceLocator.Keys)
        {
            if (amd.resourceLocator.Locate(k, typeof(T), out var locs))
            {
                foreach (var l in locs)
                {
                    list.Add(l.PrimaryKey);
                    //list.Add($"{l.PrimaryKey} ({l.ResourceType.Name}) ({l.InternalId})");
                }
            }
        }

        return list.ToList();
    }
    
    public override List<string> GetAssetListPaginated(int page = 0, int pageCount = 100)
    {
        // TODO: Fix. (?)
        var lmd = ModDefinition as AddressablesLoadedModDefinition;
        
        var strList = new List<string>();

        int startCnt = pageCount * (page);
        int endCnt = pageCount * (page + 1);
        int cnt = 0;
        
        foreach (var k in lmd.resourceLocator.Keys)
        {
            if (k == null) continue;
            if (cnt < startCnt)
            {
                cnt++;
                continue;
            }

            if (cnt >= endCnt) break;
            
            strList.Add(k as string);
            cnt++;
        }
        return strList;
    }

    public override List<string> GetLoadedAssetList()
    {
        var str = new List<string>();
        foreach (var la in loadedAssetList)
        {
            str.Add(la.Key);
        }
        return str;
    }

    public override List<string> GetLoadedAssetListByType<T>()
    {
        return base.GetLoadedAssetListByType<T>();
    }

    public override bool HasAsset(string id)
    {
        var lmd = ModDefinition as AddressablesLoadedModDefinition;
        lmd.resourceLocator.Locate(id, typeof(UnityEngine.Object), out var locations);
        if (locations == null || locations.Count == 0) return false;
        return true;
    }

    public override bool IsAssetLoaded(string id)
    {
        return loadedAssetList.ContainsKey(id);
    }

    public override async UniTask<AssetLoadResult> LoadAssetByIDAsync(string id)
    {
        return await LoadAssetByIDAsync<UnityEngine.Object>(id);
    }
    
    public override async UniTask<AssetLoadResult> LoadAssetByIDAsync<T>(string id)
    {
        var loadResult = new AssetLoadResult(false, new LoadedAssetHandleWrapper()
        {
            handleType = AssetHandleType.Addressables,
            assetReference = new ModAssetSoftReference(modID, id, false)
        });
        loadResult.result = false;
        
        
        var lmd = ModDefinition as AddressablesLoadedModDefinition;
        lmd.resourceLocator.Locate(id, typeof(T), out var locations);
        if (locations == null || locations.Count == 0)
        {
            Debug.LogError($"Could not find asset location. ID={id}");
            return loadResult;
        }

        var lc = locations.First();
        var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(lc);
        await handle;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Couldn't load asset handle.");
            return loadResult;
        }

        bool definitionLoadResult = true;
        if (loadedAssetList.ContainsKey(lc.PrimaryKey) == false || loadedAssetList[lc.PrimaryKey].Count == 0)
        {
            if (handle.Result is IContentDefinition definition)
            {
                definition.modDefinition = ModDefinition;
                definitionLoadResult = await definition.Load(lc.PrimaryKey);
            }
        }

        loadResult.result = true;
        loadResult.handle.addressablesHandle = handle;
        
        RegisterAssetHandle(lc.PrimaryKey, handle);
        RegisterAssetByType(lc.PrimaryKey);
        return loadResult;
    }

    /*
    public override void RegisterQuantumAssets(string id)
    {
        if (!loadedAssetList.ContainsKey(id) || loadedAssetList[id].Count == 0 || loadedAssetList[id][0].Result is not AssetObject) return;
        QuantumUnityDB.Global.AddAsset((AssetObject)loadedAssetList[id][0].Result);
    }

    public override void UnregisterQuantumAssets(string id)
    {
        if (!loadedAssetList.ContainsKey(id) || loadedAssetList[id].Count == 0 || loadedAssetList[id][0].Result is not AssetObject) return;
        QuantumUnityDB.Global.RemoveSource((loadedAssetList[id][0].Result as AssetObject).Guid);
    }*/

    public override Object GetAssetByID(string id, bool autoLoad = false)
    {
        if (!loadedAssetList.ContainsKey(id) || loadedAssetList[id].Count == 0) return null;
        return loadedAssetList[id][0].Result as Object;
    }

    public override T GetAssetByID<T>(string id, bool autoLoad = false)
    {
        return GetAssetByID(id, autoLoad) as T;
    }

    public override List<T> GetAssetsByType<T>(bool includeInheritors = true)
    {
        List<T> assetList = new();
        foreach (var kvp in loadedAssetsByType)
        {
            if (!kvp.Key.IsAssignableFrom(typeof(T))) continue;
            assetList.AddRange((IEnumerable<T>)kvp.Value);
        }

        return assetList;
    }

    public override void ReleaseAsset(LoadedAssetHandleWrapper assetHandle)
    {
        if (!loadedAssetList.ContainsKey(assetHandle.assetReference.assetID))
        {
            Debug.LogError("Attempting to release handle for asset that hasn't been loaded.");
            return;
        }
        if (!loadedAssetList[assetHandle.assetReference.assetID].Contains(assetHandle.addressablesHandle))
        {
            Debug.LogError("Releasing handle that isn't valid anymore.");
            return;
        }
        loadedAssetList[assetHandle.assetReference.assetID].Remove(assetHandle.addressablesHandle);

        if (loadedAssetList[assetHandle.assetReference.assetID].Count == 0)
        {
            var assetAsContentDefinition = assetHandle.GetAsset<UnityEngine.Object>();
            if (assetAsContentDefinition is IContentDefinition contentDefinition)
            {
                contentDefinition.Unload();
            }

            ProfilerStats.LoadedAssetsCount.Value--;
        }
        
        Addressables.Release(assetHandle.addressablesHandle);
    }

    /*
    public override void UnloadAssetByID(string id)
    {
        if (!loadedAssetList.TryGetValue(id, out var value)) return;
        
        /*if (value is IContentDefinition definition)
        {
            definition.Unload();
        }

        var savedObject = loadedAssetList[id];
        UnregisterAssetByType(id);
        UnregisterAsset(id);
        Addressables.Release(savedObject);
    }

    public override void UnloadAssetsByType<T>(bool includeInheritors = true)
    {
        foreach (var kvp in loadedAssetsByType)
        {
            if (!kvp.Key.IsAssignableFrom(typeof(T))) continue;
            
        }
    }*/

    public override void OnUnload()
    {
        
    }

    private void RegisterAssetHandle(string id, AsyncOperationHandle assetHandle)
    {
        loadedAssetList.TryAdd(id, new List<AsyncOperationHandle>());
        loadedAssetList[id].Add(assetHandle);
    }
    
    private void RegisterAssetByType(string key)
    {
        /*
        if (!loadedAssetList.TryGetValue(key, out var asset)) return;
        loadedAssetsByType.TryAdd(asset.GetType(), new List<string>());
        loadedAssetsByType[asset.GetType()].Add(key);*/
    }

    private void AttemptUnregisterAssetByType(string key)
    {
        /*
        if (!loadedAssetList.ContainsKey(key)
            || loadedAssetList[key].Count > 0
            || !loadedAssetsByType.ContainsKey(asset.GetType())) return;*/
        //loadedAssetsByType[asset.GetType()].Remove(key);
    }
}
