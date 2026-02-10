using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Quantum;
using UMod;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Mod Definitions/UMod ModInfoAsset")]
    [System.Serializable]
    public class UModModInfoAsset : BaseModInfoAsset, IOnUModPrebuild
    {
        private UModLoadedModDefinition _modDefinition;

        public override LoadedModDefinition ModDefinition
        {
            get => _modDefinition;
            set => _modDefinition = (UModLoadedModDefinition)value;
        }

        public override string ModID => modID;
        public override string ModName => modName;

        public string modID;
        public string modName;

        /// <summary>
        /// Converts an asset path to it's custom id.
        /// If no custom ID was assigned, just returns the asset path.
        /// </summary>
        [SerializeField, HideInInspector] public SerializedDictionary<string, string> AssetPathToIDMappng = new();

        /// <summary>
        /// Converts an asset's custom id to it's asset path.
        /// </summary>
        [SerializeField, HideInInspector] public SerializedDictionary<string, string> IDToAssetMapping = new();

        /// <summary>
        /// Returns the paths of all assets in the mod that have the given type.
        /// The type string is the AssemblyQualifiedName.
        /// </summary>
        [SerializeField, HideInInspector] public SerializedDictionary<string, List<string>> TypeToAssetPaths = new();
        
        /// <summary>
        /// Keeps track of the auto created references and what asset they're pointing to.
        /// The key is the asset's GUID.
        /// </summary>
        [SerializeField]
        public SerializedDictionary<string, ExternalModAssetSoftReference> guidToAssetReference = new();

        /// <summary>
        /// Used by the mod creator to assign custom IDs to any asset in the mod.
        /// </summary>
        [SerializeField, SerializedDictionary("Identifier", "Asset")]
        public SerializedDictionary<ExternalModAssetSoftReference, string> assetIdRemap;
        
        /// <summary>
        /// Used by the mod creator to ignore the contents of any folder or asset from auto reference creation.
        /// </summary>
        public List<ExternalModAssetSoftReference> excludedAssetsFromAutoCreation = new();

        /// <summary>
        /// Content that has been loaded, sorted by their type.
        /// </summary>
        [NonSerialized] private Dictionary<Type, List<string>> _loadedAssetsByType = new();

        /// <summary>
        /// Loaded assets, indexed by their path.
        /// </summary>
        [NonSerialized] private Dictionary<string, UnityEngine.Object> _loadedAssetList = new();

        public void OnUModPrebuild()
        {
            
        }

        public void RegisterAllToGlobalDB()
        {
            //var source = new QuantumAssetObjectSourceUMod(modName, "PathToAsset", typeof(IFighterDefinition));
            //QuantumUnityDB.Global.AddSource(source, new AssetGuid());
        }

        public override void RegisterQuantumAssets(QuantumUnityDB quantumUnityDB)
        {
            base.RegisterQuantumAssets(quantumUnityDB);
        }

        public override void UnregisterQuantumAssets(QuantumUnityDB quantumUnityDB)
        {
            base.UnregisterQuantumAssets(quantumUnityDB);
        }

        public override void OnLoad()
        {
            var modAssetList = _modDefinition.modHost.Assets.FindAll();
            var msg = "UMOD Mod. Assets: \n";
            foreach (var ass in modAssetList)
            {
                msg += $"{ass.FullName} {ass.GetType().AssemblyQualifiedName} \n";
            }

            Debug.Log(msg);

            LoadAssetByID("_AutoReferences/");
        }

        public override void OnUnload()
        {
            foreach (var loadedAsset in _loadedAssetList)
            {
                UnregisterLoadedAsset(loadedAsset.Key);
            }
        }

        public override List<string> GetAssetList()
        {
            return IDToAssetMapping.Keys.ToList();
        }

        public override List<string> GetAssetListByType<T>()
        {
            var l = new List<string>();
            var assemblyQualifiedName = typeof(T).AssemblyQualifiedName;
            if (assemblyQualifiedName == null) return l;
            if (!TypeToAssetPaths.ContainsKey(assemblyQualifiedName)) return l;
            
            foreach (var assetPath in TypeToAssetPaths[typeof(T).AssemblyQualifiedName])
            {
                l.Add(ConvertAssetPathToID(assetPath));
            }
            return l;
        }

        public virtual bool TryRegisterQuantumAsset(UnityEngine.Object obj, bool printError = true)
        {
            if (obj is not AssetObject assetObject) return false;
            try
            {
                QuantumUnityDB.Global.AddAsset(assetObject);
            }
            catch (Exception e)
            {
                if(printError) Debug.LogError($"Failed to register quantum asset {obj} {obj?.name}: {e}");
                return false;
            }
            return true;
        }

        public virtual bool TryUnregisterQuantumAsset(UnityEngine.Object obj, bool printError = false)
        {
            if (obj is not AssetObject assetObject) return false;
            try
            {
                QuantumUnityDB.Global.RemoveSource(assetObject.Guid);
            }
            catch (Exception e)
            {
                if(printError) Debug.LogError($"Failed to register quantum asset {obj} {obj?.name}: {e}");
                return false;
            }
            return true;
        }
        
        /*
        public override void RegisterQuantumAssets(string id)
        {
            var assetPath = ConvertIDToAssetPath(id);
            
            if (!_loadedAssetList.ContainsKey(id)
                || _loadedAssetList[id] is not AssetObject) return;
        
            QuantumUnityDB.Global.AddAsset((AssetObject)_loadedAssetList[id]);
        }

        public override void UnregisterQuantumAssets(string id)
        {
            var assetPath = ConvertIDToAssetPath(id);
            
            if (!_loadedAssetList.ContainsKey(id)
                || _loadedAssetList[id] is not AssetObject) return;

            QuantumUnityDB.Global.RemoveSource((_loadedAssetList[id] as AssetObject).Guid);
        } */
        
        public override List<string> GetLoadedAssetList()
        {
            var l = new List<string>();
            foreach (var assetPath in _loadedAssetList.Keys)
                l.Add(ConvertAssetPathToID(assetPath));
            return l;
        }
        
        public override List<string> GetLoadedAssetListByType<T>()
        {
            var l = new List<string>();
            var assemblyQualifiedName = typeof(T).AssemblyQualifiedName;
            if (assemblyQualifiedName == null) return l;

            foreach (var assetPathToObject in _loadedAssetList)
            {
                if(assetPathToObject.Value is T) l.Add(ConvertAssetPathToID(assetPathToObject.Key));
            }
            return l;
        }

        public override bool HasAsset(string id)
        {
            var assetPath = ConvertIDToAssetPath(id);
            var modHostAssets = _modDefinition.modHost.Assets;
            return modHostAssets.Find(assetPath) != null;
        }
        
        public string ConvertIDToAssetPath(string id)
        {
            return IDToAssetMapping.ContainsKey(id) ? IDToAssetMapping[id] : id;
        }
        
        public string ConvertAssetPathToID(string assetPath)
        {
            return AssetPathToIDMappng.ContainsKey(assetPath) ? AssetPathToIDMappng[assetPath] : assetPath;
        }

        private bool PathIsFolder(string assetPath)
        {
            return !assetPath.Split("/")[^1].Contains(".");
        }

        private bool AssetPathWithinFolder(string folderPath, string assetPath)
        {
            return folderPath.Length < assetPath.Length && assetPath.StartsWith(folderPath);
        }
        
        public bool LoadAssetByID(string id)
        {
            var assetPath = ConvertIDToAssetPath(id);
            var modHostAssets = _modDefinition.modHost.Assets;
            
            if (PathIsFolder(assetPath))
            {
                var assetsInFolder = modHostAssets.FindAllInFolder(assetPath);

                foreach (var folderAsset in assetsInFolder)
                {
                    var folderAssetPath = folderAsset.RelativeName + folderAsset.Extension;
                    if (_loadedAssetList.ContainsKey(folderAssetPath)) continue;
                    
                    var loadOperation = folderAsset.LoadWithSubAssets(allowCaching: true);
                    if (loadOperation == null || loadOperation.Length == 0) continue;
                    var assetObject = modHostAssets.Find(folderAsset.RelativeName).AssetObject;
                    _ = RegisterLoadedAsset(folderAssetPath, assetObject);
                }
                return true;
            }
            
            if (_loadedAssetList.ContainsKey(assetPath)) return true;
            var op = modHostAssets.Load(assetPath);
            if (op == null) return false;
            _ = RegisterLoadedAsset(assetPath, modHostAssets.Find(assetPath).AssetObject);
            return true;
        }

        public override async UniTask<AssetLoadResult> LoadAssetByIDAsync(string id)
        {
            var loadResult = new AssetLoadResult(false, default);
            loadResult.handle.handleType = AssetHandleType.UMod;
            var assetPath = ConvertIDToAssetPath(id);
            var modHostAssets = _modDefinition.modHost.Assets;
            
            if (PathIsFolder(assetPath))
            {
                var assetsInFolder = modHostAssets.FindAllInFolder(assetPath);

                foreach (var folderAsset in assetsInFolder)
                {
                    var folderAssetPath = folderAsset.RelativeName + folderAsset.Extension;
                    if (_loadedAssetList.ContainsKey(folderAssetPath)) continue;
                    
                    var loadOperation = folderAsset.LoadWithSubAssetsAsync(allowCaching: true);
                    await loadOperation;
                    if (!loadOperation.IsSuccessful) continue;
                    var assetObject = modHostAssets.Find(folderAsset.RelativeName).AssetObject;
                    await RegisterLoadedAsset(folderAssetPath, assetObject);
                }

                loadResult.result = true;
                // TODO: Handles.
                return loadResult;
            }

            if (_loadedAssetList.ContainsKey(assetPath))
            {
                // TODO: Handles.
                loadResult.handle = new LoadedAssetHandleWrapper()
                {
                    handleType = AssetHandleType.UMod
                };
                return loadResult;
            }
            var op = modHostAssets.LoadAsync(assetPath);
            await op;
            if (!op.IsSuccessful)
            {
                loadResult.result = false;
                return loadResult;
            }

            loadResult.handle.umodHandle = op;
            loadResult.handle.assetReference = new ModAssetSoftReference(modID, id, false);
            
            await RegisterLoadedAsset(assetPath, modHostAssets.Find(assetPath).AssetObject);
            return loadResult;
        }

        public override T GetAssetByID<T>(string id, bool autoLoad = false)
        {
            return (T)GetAssetByID(id);
        }

        public override Object GetAssetByID(string id, bool autoLoad = false)
        {
            var assetPath = ConvertIDToAssetPath(id);
            if (autoLoad && !LoadAssetByID(assetPath)) return null;
            return _loadedAssetList.GetValueOrDefault(assetPath);
        }

        public virtual List<UnityEngine.Object> GetAssetsByID(string id)
        {
            List<UnityEngine.Object> result = new();
            var assetPath = ConvertIDToAssetPath(id);
            if (PathIsFolder(assetPath))
            {
                foreach (var ld in _loadedAssetList)
                {
                    if(!AssetPathWithinFolder(assetPath, ld.Key)) continue;
                    result.Add(ld.Value);
                }
            }
            else
            {
                if (_loadedAssetList.TryGetValue(assetPath, out var value)) result.Add(value);
            }
            return result;
        }

        public Object GetLoadedAssetByPath(string path)
        {
            return _loadedAssetList.GetValueOrDefault(path);
        }

        public override List<T> GetAssetsByType<T>(bool includeInheritors = true)
        {
            List<T> assetList = new();
            var t = typeof(T).AssemblyQualifiedName;
            if (string.IsNullOrEmpty(t)) return assetList;
            if (!TypeToAssetPaths.ContainsKey(t)) return assetList;
            
            foreach (var assetPath in TypeToAssetPaths[typeof(T).AssemblyQualifiedName])
            {
                var asset = (T)GetLoadedAssetByPath(assetPath);
                if (asset != null) assetList.Add(asset);
            }

            return assetList;
        }

        public override System.Object GetAssetInfo(string id)
        {
            var modHostAssets = _modDefinition.modHost.Assets;
            var assetPath = ConvertIDToAssetPath(id);
            var modAsset = modHostAssets.Find(assetPath);
            return modAsset;
        }

        /*
        public override void UnloadAssetByID(string id)
        {
        }

        public override void UnloadAssetsByType<T>(bool includeInheritors = true)
        {
        }*/

        public override void ReleaseAsset(LoadedAssetHandleWrapper assetHandle)
        {
            base.ReleaseAsset(assetHandle);
            // TODO
        }

        public List<ModAsyncOperation> GetAssetHandlesFromAssetId(string id)
        {
            List<ModAsyncOperation> handles = new List<ModAsyncOperation>();
            var assetPath = ConvertIDToAssetPath(id);
            var modHostAssets = _modDefinition.modHost.Assets;
            
            if (PathIsFolder(assetPath))
            {
                var assetsInFolder = modHostAssets.FindAllInFolder(assetPath);

                foreach (var folderAsset in assetsInFolder)
                {
                    handles.Add(folderAsset.LoadWithSubAssetsAsync());
                }
            }
            else
            {
                var handle = modHostAssets.LoadAsync(assetPath);
                handles.Add(handle);
            }

            return handles;
        }

        private async UniTask RegisterLoadedAsset(string assetPath, Object assetObject)
        {
            if (assetObject is IContentDefinition definition)
            {
                definition.modDefinition = ModDefinition;
                await definition.Load(assetPath);
            }
            
            _loadedAssetList.TryAdd(assetPath, assetObject);
            TryRegisterQuantumAsset(assetObject);
        }

        private void UnregisterLoadedAsset(string assetPath)
        {
            if (!_loadedAssetList.ContainsKey(assetPath)) return;
            TryUnregisterQuantumAsset(_loadedAssetList[assetPath]);
        }
    }
}