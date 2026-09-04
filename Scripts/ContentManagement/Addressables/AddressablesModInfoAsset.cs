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
public class AddressablesModInfoAsset : BaseModInfoAsset, ILoadedAssetHandleOwner
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
    [NonSerialized] private Dictionary<string, HashSet<LoadedAssetHandleWrapper>> loadedAssetList = new();

    // ...
    [NonSerialized] private bool isUnloading;
    [NonSerialized] private readonly Dictionary<string, UniTaskCompletionSource<bool>> assetInitializations = new();
    [NonSerialized] private int activeLoadCount;
    [NonSerialized] private UniTaskCompletionSource<bool> allLoadsFinished;

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
        isUnloading = false;
        activeLoadCount = 0;
        allLoadsFinished = null;

        assetInitializations.Clear();
        loadedAssetList.Clear();
        loadedAssetsByType.Clear();

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
        return loadedAssetList
            .Where(pair => pair.Value.Count > 0)
            .Select(pair => pair.Key)
            .ToList();
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
        return loadedAssetList.TryGetValue(id, out var leases) && leases.Count > 0;
    }

    public override UniTask<LoadedAssetHandleWrapper> LoadAssetByIDAsync(string id)
    {
        return LoadAssetByIDAsync<UnityEngine.Object>(id);
    }

    public override UniTask<LoadedAssetHandleWrapper> LoadAssetByIDAsync<T>(string id)
    {
        return LoadAssetByIDAsyncInternal<T>(id);
    }

    private async UniTask<LoadedAssetHandleWrapper> LoadAssetByIDAsyncInternal<T>(string id)
        where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("Cannot load an asset with an empty ID.");
            return null;
        }

        if (isUnloading)
        {
            Debug.LogWarning($"Cannot load '{id}' because mod '{ModID}' is unloading.");
            return null;
        }

        if (ModDefinition is not AddressablesLoadedModDefinition loadedMod || loadedMod.resourceLocator == null)
        {
            Debug.LogError($"Cannot load '{id}' because mod '{ModID}' is not initialized.");
            return null;
        }

        if (!loadedMod.resourceLocator.Locate(id, typeof(T), out var locations)
            || locations == null
            || locations.Count == 0)
        {
            Debug.LogError($"Could not locate asset. Mod={ModID}, ID={id}, Type={typeof(T).Name}");
            return null;
        }

        var location = locations[0];
        string canonicalKey = location.PrimaryKey;

        if (!TryBeginLoad())
        {
            Debug.LogWarning($"Cannot load '{id}' because mod '{ModID}' is unloading.");
            return null;
        }

        AsyncOperationHandle addressablesHandle = default;
        bool leaseGiven = false;

        try
        {
            addressablesHandle = Addressables.LoadAssetAsync<T>(location);

            await addressablesHandle;

            if (addressablesHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Could not load asset. Mod={ModID}, " +
                               $"ID={id}, Key={canonicalKey}, " +
                               $"Exception={addressablesHandle.OperationException}");

                return null;
            }

            if (isUnloading)
                return null;

            bool initializationSucceeded = await EnsureContentInitializedAsync(canonicalKey, addressablesHandle.Result);

            if (!initializationSucceeded)
                return null;

            if (isUnloading)
                return null;

            var reference = new ModAssetSoftReference(ModID, canonicalKey, false);

            var lease = new LoadedAssetHandleWrapper(this, reference, addressablesHandle);

            if (!loadedAssetList.TryGetValue(canonicalKey, out var leases))
            {
                Debug.LogError($"Initialized asset has no lease collection. Key={canonicalKey}");
                return null;
            }

            bool isFirstLease = leases.Count == 0;

            if (!leases.Add(lease))
                return null;

            leaseGiven = true;

            if (isFirstLease)
            {
                ProfilerStats.LoadedAssetsCount.Value++;
                RegisterAssetByType(canonicalKey, addressablesHandle.Result?.GetType());
            }

            return lease;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Exception loading asset. Mod={ModID}, ID={id}, Key={canonicalKey}\n" + exception);
            return null;
        }
        finally
        {
            if (!leaseGiven && addressablesHandle.IsValid())
            {
                Addressables.Release(addressablesHandle);
            }

            EndLoad();
        }
    }

    private async UniTask<bool> EnsureContentInitializedAsync(string canonicalKey, System.Object asset)
    {
        // Already loaded.
        if (loadedAssetList.ContainsKey(canonicalKey))
            return true;

        // Currently being loaded.
        if (assetInitializations.TryGetValue(canonicalKey, out var existingInitialization))
            return await existingInitialization.Task;

        // First load.
        var initialization = new UniTaskCompletionSource<bool>();
        assetInitializations.Add(canonicalKey, initialization);

        bool succeeded = false;

        try
        {
            if (isUnloading)
                return false;

            if (asset is IContentDefinition definition)
            {
                definition.modDefinition = ModDefinition;
                succeeded = await definition.Load(canonicalKey);

                if (!succeeded)
                {
                    definition.Unload();
                    return false;
                }

                if (isUnloading)
                {
                    definition.Unload();
                    succeeded = false;
                    return false;
                }
            }
            else
            {
                succeeded = true;
            }

            if (isUnloading)
            {
                succeeded = false;
                return false;
            }

            loadedAssetList.TryAdd(canonicalKey, new HashSet<LoadedAssetHandleWrapper>());
            return true;
        }
        catch (Exception exception)
        {
            if (asset is IContentDefinition definition)
            {
                try
                {
                    definition.Unload();
                }
                catch (Exception unloadException)
                {
                    Debug.LogException(unloadException);
                }
            }

            Debug.LogError($"Content initialization failed. Mod={ModID}, Key={canonicalKey}\n" + exception);
            return false;
        }
        finally
        {
            initialization.TrySetResult(succeeded);
            assetInitializations.Remove(canonicalKey);
        }
    }

    public override Object GetAssetByID(string id, bool autoLoad = false)
    {
        if (!loadedAssetList.ContainsKey(id) || loadedAssetList[id].Count == 0) return null;
        return loadedAssetList[id].FirstOrDefault()?.GetAsset();
    }

    public override T GetAssetByID<T>(string id, bool autoLoad = false)
    {
        return GetAssetByID(id, autoLoad) as T;
    }

    public override List<T> GetAssetsByType<T>(bool includeInheritors = true)
    {
        var results = new List<T>();

        foreach (var pair in loadedAssetsByType)
        {
            bool isTypeValid = includeInheritors ? typeof(T).IsAssignableFrom(pair.Key) : pair.Key == typeof(T);

            if (!isTypeValid)
                continue;

            foreach (string assetId in pair.Value)
            {
                T asset = GetAssetByID<T>(assetId);
                if (asset != null)
                    results.Add(asset);
            }
        }
        
        return results;
    }

    public void Release(LoadedAssetHandleWrapper handle)
    {
        ReleaseAsset(handle);
    }

    public override void ReleaseAsset(LoadedAssetHandleWrapper lease)
    {
        if (lease == null)
            return;

        string key = lease.AssetReference.assetID;

        if (!loadedAssetList.TryGetValue(key, out var leases) || !leases.Remove(lease))
        {
            Debug.LogWarning($"Lease is no longer registered. Mod={ModID}, Key={key}");
            return;
        }

        bool wasLastLease = leases.Count == 0;

        try
        {
            if (wasLastLease && lease.AddressablesHandle.Result is IContentDefinition definition)
            {
                definition.Unload();
            }
        }
        finally
        {
            if (wasLastLease)
            {
                loadedAssetList.Remove(key);
                AttemptUnregisterAssetByType(key, lease.AddressablesHandle.Result.GetType());
                ProfilerStats.LoadedAssetsCount.Value--;
            }
            
            if (lease.AddressablesHandle.IsValid())
                Addressables.Release(lease.AddressablesHandle);
        }
    }


    public override void ReleaseAll()
    {
        var leases = loadedAssetList.Values
            .SelectMany(collection => collection)
            .ToArray();

        List<Exception> exceptions = null;

        foreach (var lease in leases)
        {
            try
            {
                lease.Dispose();
            }
            catch (Exception exception)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(exception);
            }
        }

        loadedAssetList.Clear();
        loadedAssetsByType.Clear();

        if (exceptions != null)
            Debug.LogException(new AggregateException(exceptions));
    }

    public override void OnUnload()
    {
        isUnloading = true;
    }

    public async UniTask PrepareForUnloadAsync()
    {
        OnUnload();

        if (activeLoadCount > 0 && allLoadsFinished != null)
            await allLoadsFinished.Task;

        UnregisterQuantumAssets(QuantumUnityDB.Global);
        ReleaseAll();
    }

    private bool TryBeginLoad()
    {
        if (isUnloading)
            return false;

        if (activeLoadCount == 0)
            allLoadsFinished = new UniTaskCompletionSource<bool>();

        activeLoadCount++;
        return true;
    }

    private void EndLoad()
    {
        if (activeLoadCount <= 0)
            return;

        activeLoadCount--;
        if (activeLoadCount != 0)
            return;

        allLoadsFinished?.TrySetResult(true);
        allLoadsFinished = null;
    }

    private void RegisterAssetByType(string assetID, Type assetType)
    {
        if (assetType == null)
            return;

        if (!loadedAssetsByType.TryGetValue(assetType, out var assetIDs))
        {
            assetIDs = new List<string>();
            loadedAssetsByType.Add(assetType, assetIDs);
        }

        if (!assetIDs.Contains(assetID))
            assetIDs.Add(assetID);
    }

    private void AttemptUnregisterAssetByType(string assetID, Type assetType)
    {
        if (string.IsNullOrEmpty(assetID))
            return;

        if (!loadedAssetsByType.TryGetValue(assetType, out var assetIDs))
            return;

        assetIDs.Remove(assetID);
        if (assetIDs.Count == 0)
            loadedAssetsByType.Remove(assetType);
    }
}