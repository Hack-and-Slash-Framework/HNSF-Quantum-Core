using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public class ModContentManager : MonoBehaviour
    {
        public ModManager modManager;

        public void Init()
        {

        }

        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public void RegisterAll()
        {
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                lmd.modAsset.RegisterQuantumAssets(QuantumUnityDB.Global);
            }
        }

        public void PrintAssetList()
        {
            int assetCounter = 0;
            string s = "Assets Available: \n";
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                var assetList = lmd.modAsset.GetAssetList();
                foreach (var asset in assetList)
                {
                    s += asset + "\n";
                }

                assetCounter += assetList.Count;
            }

            s += "Total Assets: " + assetCounter;
            Debug.Log(s);
        }

        public void PrintAssetList<T>() where T : UnityEngine.Object
        {
            int assetCounter = 0;
            string s = $"Assets Available of type {typeof(T).AssemblyQualifiedName}: \n";
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                var assetList = lmd.modAsset.GetAssetListByType<T>();
                foreach (var asset in assetList)
                {
                    s += asset + "\n";
                }

                assetCounter += assetList.Count;
            }

            s += "Total Assets: " + assetCounter;
            Debug.Log(s);
        }

        public void PrintLoadedAssetList()
        {
            string s = "Assets Available: \n";
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                var assetList = lmd.modAsset.GetLoadedAssetList();
                foreach (var asset in assetList)
                {
                    s += asset + "\n";
                }
            }

            Debug.Log(s);
        }

        public List<string> GetAssetList()
        {
            var assetList = new List<string>();
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                if (lmd.modAsset == null) continue;
                assetList.AddRange(lmd.modAsset.GetAssetList());
            }

            return assetList;
        }

        public List<ModAssetSoftReference> GetAssetList<T>() where T : UnityEngine.Object
        {
            var assetList = new List<ModAssetSoftReference>();

            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                if (lmd.modAsset == null) continue;
                foreach (var assetId in lmd.modAsset.GetAssetListByType<T>())
                {
                    assetList.Add(new ModAssetSoftReference(lmd.modAsset.ModID, assetId, false));
                }
            }

            return assetList;
        }

        public (List<ModAssetSoftReference>, int) GetAssetListPaginated<T>(int amountPerPage = 10, int page = 0)
            where T : UnityEngine.Object
        {
            var assetList = new List<ModAssetSoftReference>();
            if (amountPerPage <= 0)
            {
                Debug.LogError($"Page size is {amountPerPage} when it needs to be more than zero.");
                return (assetList, 0);
            }

            int startIndex = page * amountPerPage;
            int currentIndex = 0;

            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                if (lmd.modAsset == null) continue;
                var modAssetList = lmd.modAsset.GetAssetListByType<T>();
                foreach (var modAssetId in modAssetList)
                {
                    currentIndex += amountPerPage;
                    if (currentIndex < startIndex) continue;
                    if (assetList.Count == amountPerPage) break;

                    assetList.Add(new ModAssetSoftReference(lmd.modAsset.ModID, modAssetId, false));
                }

                if (assetList.Count == amountPerPage) break;
            }

            int actualPage = (currentIndex / amountPerPage);

            return (assetList, actualPage);
        }

        public bool ModHasAsset(LoadedModDefinition loadedModDefinition, string assetID)
        {
            return loadedModDefinition.modAsset.HasAsset(assetID);
        }

        public bool ModHasAsset(string modID, string assetID)
        {
            var modDefinition = modManager.GetMod(modID);
            if (modDefinition == null) return false;
            var loadedModDefinition = modDefinition.loadedDefinition;
            return loadedModDefinition.modAsset.HasAsset(assetID);
        }

        public bool IsAssetLoaded(ModAssetSoftReference assetReference)
        {
            return IsAssetLoaded(assetReference.mod, assetReference.assetID);
        }

        public bool IsAssetLoaded(string modId, string assetId)
        {
            var modDefinition = modManager.GetMod(modId);
            if (modDefinition?.loadedDefinition == null
                || modDefinition.loadedDefinition.modAsset == null) return false;
            return IsAssetLoaded(modDefinition.loadedDefinition, assetId);
        }

        public bool IsAssetLoaded(LoadedModDefinition loadedModDefinition, string assetId)
        {
            return loadedModDefinition.modAsset.IsAssetLoaded(assetId);
        }

        public List<string> GetLoadedAssetList()
        {
            var loadedAssetList = new List<string>();
            foreach (var lmd in modManager.currentlyLoadedMods)
            {
                if (lmd.modAsset == null) continue;
                loadedAssetList.AddRange(lmd.modAsset.GetLoadedAssetList());
            }

            return loadedAssetList;
        }

        public async UniTask<List<LoadedAssetHandleWrapper>> LoadAllAssetsFromModAsync(LoadedModDefinition loadedModDefinition)
        {
            var modAssetList = loadedModDefinition.modAsset?.GetAssetList();
            if (modAssetList == null) return null;

            var l = new List<LoadedAssetHandleWrapper>();
            
            foreach (var modAssetRef in modAssetList)
            {
                var handle = await loadedModDefinition.modAsset.LoadAssetByIDAsync(modAssetRef);
                if(handle != null)
                    l.Add(handle);
            }
            
            return l;
        }

        public async UniTask<List<LoadedAssetHandleWrapper>> LoadAllAssetsByTypeAsync<T>() where T : UnityEngine.Object
        {
            var l = new List<LoadedAssetHandleWrapper>();
            
            foreach (var loadedModDefinition in modManager.currentlyLoadedMods)
            {
                var modAssetList = loadedModDefinition.modAsset?.GetAssetListByType<T>();
                if (modAssetList == null) continue;

                foreach (var modAssetRef in modAssetList)
                {
                    var handle = await loadedModDefinition.modAsset.LoadAssetByIDAsync(modAssetRef);
                    if(handle != null)
                        l.Add(handle);
                }
            }

            return l;
        }

        public async UniTask<List<LoadedAssetHandleWrapper>> LoadAllAssetsFromModByTypeAsync<T>(LoadedModDefinition loadedModDefinition) where T : UnityEngine.Object
        {
            var modAssetList = loadedModDefinition.modAsset?.GetAssetListByType<T>();
            if (modAssetList == null) return null;

            var l = new List<LoadedAssetHandleWrapper>();
            
            foreach (var modAssetRef in modAssetList)
            {
                var handle = await loadedModDefinition.modAsset.LoadAssetByIDAsync(modAssetRef);
                if(handle != null)
                    l.Add(handle);
            }

            return l;
        }

        public async UniTask<LoadedAssetHandleWrapper> LoadAssetFromModAsync(ModAssetSoftReference loadedModDefinition)
        {
            return await LoadAssetFromModAsync(loadedModDefinition.mod, loadedModDefinition.assetID);
        }

        public async UniTask<LoadedAssetHandleWrapper> LoadAssetFromModAsync(LoadedModDefinition loadedModDefinition,
            string assetID)
        {
            if (loadedModDefinition.modAsset == null) return null;
            return await loadedModDefinition.modAsset.LoadAssetByIDAsync(assetID);
        }

        public async UniTask<LoadedAssetHandleWrapper> LoadAssetFromModAsync(string modID, string assetID)
        {
            var modDefinition = modManager.GetMod(modID);
            if (modDefinition?.loadedDefinition == null
                || modDefinition.loadedDefinition.modAsset == null)
            {
                Debug.LogError("Mod Asset Not Loaded.");
                return null;
            }

            return await modDefinition.loadedDefinition.modAsset.LoadAssetByIDAsync(assetID);
        }

        public async UniTask<List<LoadedAssetHandleWrapper>> LoadAssetFromModsAsync(string assetID)
        {
            var l = new List<LoadedAssetHandleWrapper>();
            
            foreach (var loadedModDefinition in modManager.currentlyLoadedMods)
            {
                var handle = await LoadAssetFromModAsync(loadedModDefinition, assetID);
                if(handle != null)
                    l.Add(handle);
            }

            return l;
        }

        public UnityEngine.Object GetAssetFromMod(ModAssetSoftReference softReference, bool autoLoad = false)
        {
            var modDefinition = modManager.GetMod(softReference.mod);
            if (modDefinition?.loadedDefinition == null) return null;
            return GetAssetFromMod(modDefinition.loadedDefinition, softReference.assetID, autoLoad);
        }

        public UnityEngine.Object GetAssetFromMod(LoadedModDefinition loadedModDefinition, string assetID,
            bool autoLoad = false)
        {
            return loadedModDefinition.modAsset.GetAssetByID(assetID, autoLoad);
        }

        public T GetAssetFromMod<T>(ModAssetSoftReference softReference) where T : UnityEngine.Object
        {
            return GetAssetFromMod<T>(softReference.mod, softReference.assetID);
        }

        public T GetAssetFromMod<T>(LoadedModDefinition loadedModDefinition, string assetID, bool autoLoad = false)
            where T : UnityEngine.Object
        {
            return loadedModDefinition.modAsset.GetAssetByID<T>(assetID, autoLoad);
        }

        public T GetAssetFromMod<T>(string modId, string assetID, bool autoLoad = false) where T : UnityEngine.Object
        {
            var modDefinition = modManager.GetMod(modId);
            if (modDefinition?.loadedDefinition == null
                || modDefinition.loadedDefinition.modAsset == null) return null;
            return modDefinition.loadedDefinition.modAsset.GetAssetByID<T>(assetID, autoLoad);
        }

        public List<UnityEngine.Object> GetAssetFromMods(string assetID, bool autoLoad = false)
        {
            List<UnityEngine.Object> assets = new();

            foreach (var loadedModDefinition in modManager.currentlyLoadedMods)
            {
                var asset = GetAssetFromMod(loadedModDefinition, assetID, autoLoad);
                if (asset == null) continue;
                assets.Add(asset);
            }

            return assets;
        }
        
        public void ReleaseAssetFromMod(LoadedAssetHandleWrapper assetHandle)
        {
            assetHandle?.Dispose();
        }
    }
}