using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    public class ModManager : MonoBehaviour
    {
        public static readonly string ModProfilesFilename = "modprofiles.json";
        public static readonly string LocalModPathIdentifier = "$LocalMod";
        public string ModDirectory { get; private set; } = "";

        public Dictionary<string, AvailableModDefinition> PathToAvailableMod = new();
        public Dictionary<string, AvailableModDefinition> IdentifierToAvailableMod = new();
        public Dictionary<int, BaseModLoader> modLoaders = new();

        public List<AvailableModDefinition> availableMods = new();
        public List<LoadedModDefinition> currentlyLoadedMods = new();

        public List<ModProfile> modProfiles = new();
        public int currentModProfileIndex = -1;
        
        public async UniTask Init()
        {
            await Addressables.InitializeAsync();
            GetModLoaders();
            ModDirectory = Application.persistentDataPath + "/mods";

            if (!Directory.Exists(ModDirectory)) Directory.CreateDirectory(ModDirectory);

            LoadModProfiles();
            await FindAvailableMods();

            Debug.Log($"Mod Loader Initialization. Available Mods: {availableMods.Count}\n" +
                      $"Mod Directory: {ModDirectory}\n" +
                      $"Mod Loaders Count: {modLoaders.Count}");
            
            foreach (var m in availableMods)
            {
                if (m.loader != (int)KnownModLoaderTypes.ADDRESSABLES_LOCAL) continue;
                await LoadMod(m);
            }
            
            //Quantum.Statics.ReInit();
            var modGuid = GenerateModGuid().ToString();
            Debug.Log($"ModString: {modGuid}");
            Debug.Log($"Loaded Mod List (GUID):\n{String.Join("\n", GetModListByGuidAsStringsWithName())}");
        }

        public void OnDestroy()
        {
            UnloadAllMods();
        }

        private void GetModLoaders()
        {
            List<string> modLoaderNames = new();
            modLoaders.Clear();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = a.GetTypes()
                    .Where(t => typeof(BaseModLoader).IsAssignableFrom(t) && t != typeof(BaseModLoader));
                foreach (var t in types)
                {
                    BaseModLoader bml = (BaseModLoader)Activator.CreateInstance(t);
                    if (modLoaders.TryAdd(bml.LoaderType, bml)) modLoaderNames.Add(bml.GetType().Name);
                }
            }

            Debug.Log($"Mod Loaders Available: {modLoaders.Count}\n{string.Join(",\n", modLoaderNames)}");
        }

        protected virtual async UniTask FindAvailableMods()
        {
            List<string> currentAvailbleModPaths = PathToAvailableMod.Keys.ToList();
            string[] foldersInDirectory = Directory.GetDirectories(ModDirectory);

            var foundLocalModPaths = await FindAddressableLocalMods();
            foreach (var localModPath in foundLocalModPaths) currentAvailbleModPaths.Remove(localModPath);

            foreach (var folderPath in foldersInDirectory)
            {
                string infoFilePath = Path.Combine(folderPath, "info.json");
                if (File.Exists(infoFilePath) == false)
                {
                    Debug.LogError($"Mod folder {infoFilePath} has no info.json file.");
                    continue;
                }

                if (!FileSaveLoadService.TryLoadFileFromJson(infoFilePath, out AvailableModDefinition amd))
                {
                    Debug.LogError($"Mod folder {infoFilePath}: Couldn't load info.json file.");
                    continue;
                }

                amd.path = folderPath;

                AddAvailableMod(amd);
                currentAvailbleModPaths.Remove(folderPath);
            }

            foreach (var invalidPath in currentAvailbleModPaths)
            {
                RemoveAvailableMod(PathToAvailableMod[invalidPath]);
            }
        }

        protected virtual async UniTask<List<string>> FindAddressableLocalMods()
        {
            List<string> foundPaths = new List<string>();
            
            IResourceLocator localResourceLocator = null;
            foreach (var rl in Addressables.ResourceLocators)
            {
                if (rl.LocatorId == "AddressablesMainContentCatalog"
                    || rl.LocatorId == "AddressableAssetSettings")
                {
                    localResourceLocator = rl; 
                    break;
                }
            }

            if (localResourceLocator == null)
            {
                Debug.LogError("No local resource locator found.");
                return foundPaths;
            }
            
            List<AsyncOperationHandle<AddressablesModInfoAsset>> gotHandles =
                new List<AsyncOperationHandle<AddressablesModInfoAsset>>();
            
            foreach (var k in localResourceLocator.Keys)
            {
                if (localResourceLocator.Locate(k, typeof(AddressablesModInfoAsset), out var locs))
                {
                    foreach (var l in locs)
                    {
                        if(l.ResourceType != typeof(AddressablesModInfoAsset) && l.ResourceType != typeof(BaseModInfoAsset)) continue;
                        gotHandles.Add(Addressables.LoadAssetAsync<AddressablesModInfoAsset>(l));
                    }
                }
            }

            for (int i = 0; i < gotHandles.Count; i++)
            {
                var result = await gotHandles[i];
                if (result == null || IdentifierToAvailableMod.ContainsKey(result.ModID)) continue;

                var path = $"{LocalModPathIdentifier}/{result.modGuid}_{result.ModID}";
                
                AddAvailableMod(new AvailableModDefinition()
                {
                    canUnload = false,
                    author = result.modAuthor,
                    guid = result.modGuid,
                    identifier = result.ModID,
                    loadedDefinition = null,
                    loader = (int)KnownModLoaderTypes.ADDRESSABLES_LOCAL,
                    name = result.ModName,
                    version = result.ModVersion,
                    path = path,
                    OnlineRequirement = ModOnlineRequirement.RequiredByAllPlayers,
                    requiresReload = false
                });
                
                foundPaths.Add(path);
            }

            return foundPaths;
        }

        private bool AddAvailableMod(AvailableModDefinition result)
        {
            if (!PathToAvailableMod.TryAdd(result.path, result)) return false;
            availableMods.Add(result);
            IdentifierToAvailableMod.Add(result.identifier, result);
            return true;
        }

        private void RemoveAvailableMod(AvailableModDefinition modDefinition)
        {
            UnloadMod(modDefinition.loadedDefinition);
            IdentifierToAvailableMod.Remove(modDefinition.identifier);
            PathToAvailableMod.Remove(modDefinition.path);
            availableMods.Remove(modDefinition);
        }

        public async UniTask<bool> LoadMod(AvailableModDefinition modDefinition)
        {
            if (!modLoaders.ContainsKey(modDefinition.loader)) return false;
            var lmd = await modLoaders[modDefinition.loader].TryLoadMod(this, modDefinition);
            if (lmd != null)
            {
                modDefinition.loadedDefinition = lmd;
                currentlyLoadedMods.Add(lmd);
                return true;
            }

            return false;
        }

        public async UniTask<bool> LoadModByPath(string path)
        {
            if (!PathToAvailableMod.ContainsKey(path)) return false;
            return await LoadMod(PathToAvailableMod[path]);
        }

        public async UniTask LoadMods(AvailableModDefinition[] modDefinitions)
        {
            foreach (var md in modDefinitions)
            {
                await LoadMod(md);
            }
        }

        public async UniTask LoadMods(string[] modIdentifiers)
        {
            foreach (var mid in modIdentifiers)
            {
                if (!IdentifierToAvailableMod.ContainsKey(mid)) continue;
                await LoadMod(IdentifierToAvailableMod[mid]);
            }
        }

        public bool UnloadMod(LoadedModDefinition loadedModDefinition)
        {
            if(loadedModDefinition == null) return false;
            if (!modLoaders.ContainsKey(loadedModDefinition.information.loader)) return false;
            return modLoaders[loadedModDefinition.information.loader].TryUnloadMod(this, loadedModDefinition);
        }

        public void UnloadMods(string[] modIdentifiers)
        {
            foreach (var md in modIdentifiers)
            {
                if (!IdentifierToAvailableMod.ContainsKey(md)
                    || IdentifierToAvailableMod[md].loadedDefinition == null) continue;
                UnloadMod(IdentifierToAvailableMod[md].loadedDefinition);
            }
        }

        public async UniTask LoadAllMods()
        {
            foreach (var amd in availableMods)
            {
                await LoadMod(amd);
            }
        }

        public void UnloadAllMods(string[] modsToExcludeByIdentifier = null)
        {
            foreach (var lmd in currentlyLoadedMods)
            {
                if (lmd.information.loader == (int)KnownModLoaderTypes.ADDRESSABLES_LOCAL) continue;
                if (modsToExcludeByIdentifier != null
                    && modsToExcludeByIdentifier.Contains(lmd.information.identifier)) continue;
                UnloadMod(lmd);
            }
        }

        public List<AvailableModDefinition> GetModsByIdentifiers(string[] modIdentifiers)
        {
            List<AvailableModDefinition> modList = new();
            foreach (var mi in modIdentifiers)
            {
                if (!IdentifierToAvailableMod.TryGetValue(mi, out var value)) continue;
                modList.Add(value);
            }

            return modList;
        }

        public AvailableModDefinition GetMod(string modIdentifier)
        {
            return IdentifierToAvailableMod.GetValueOrDefault(modIdentifier);
        }
        
        public async UniTask ApplyModProfile(ModProfile modProfile)
        {
            UnloadAllMods(modProfile.modsByIdentifiers.ToArray());
            await LoadMods(modProfile.modsByIdentifiers.ToArray());
            currentModProfileIndex = modProfiles.IndexOf(modProfile);
        }

        public void LoadModProfiles()
        {
            if (!FileSaveLoadService.TryLoadFileFromJson(ModProfilesFilename, out modProfiles))
            {
                Debug.Log("Couldn't load mod profile. Creating new one.");
                modProfiles = new List<ModProfile>();
                modProfiles.Add(new ModProfile());
                SaveModProfiles();
                return;
            }
        }

        public bool IsModLoaded(string modID)
        {
            return currentlyLoadedMods.Count(x => x.information.identifier == modID) > 0;
        }

        public ModProfile GetCurrentModProfile()
        {
            if (currentModProfileIndex == -1 || modProfiles.Count == 0) return null;
            return modProfiles[currentModProfileIndex];
        }

        public bool TryGetCurrentModProfile(out ModProfile currentProfile)
        {
            currentProfile = GetCurrentModProfile();
            return currentProfile != null;
        }

        public void SaveModProfiles()
        {
            FileSaveLoadService.SaveFileAsJson(ModProfilesFilename, modProfiles);
        }

        public Guid GenerateModGuid(bool loadedOnly = true, bool onlyRequiredMods = true)
        {
            string modString = "";

            var availableModsSortedByName = availableMods
                .OrderBy(x => x.identifier)
                .ThenBy(x => x.author)
                .ThenBy(x => x.name)
                .ThenBy(x => x.version);

            foreach (var availableMod in availableModsSortedByName)
            {
                if (availableMod.loadedDefinition == null && loadedOnly) continue;
                if (onlyRequiredMods &&
                    availableMod.OnlineRequirement != ModOnlineRequirement.RequiredByAllPlayers) continue;
                modString += availableMod.guid;
            }

            MD5 md5Hasher = MD5.Create();
            byte[] hash = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(modString));
            return new Guid(hash);
        }

        public Guid[] GetModListByGuid(bool loadedOnly = true, bool onlineOnly = true)
        {
            List<Guid> guidList = new List<Guid>();

            foreach (var availableMod in availableMods)
            {
                if (availableMod.loadedDefinition == null && loadedOnly) continue;
                if (onlineOnly && availableMod.OnlineRequirement != ModOnlineRequirement.RequiredByAllPlayers) continue;
                guidList.Add(availableMod.GetGuid());
            }

            return guidList.ToArray();
        }

        public string[] GetModListByGuidAsStrings(bool loadedOnly = true, bool onlineOnly = true)
        {
            List<string> guidList = new();

            foreach (var availableMod in availableMods)
            {
                if (availableMod.loadedDefinition == null && loadedOnly) continue;
                if (onlineOnly && availableMod.OnlineRequirement != ModOnlineRequirement.RequiredByAllPlayers) continue;
                guidList.Add(availableMod.GetGuid().ToString());
            }

            return guidList.ToArray();
        }
        
        public string[] GetModListByGuidAsStringsWithName(bool loadedOnly = true, bool onlineOnly = true)
        {
            List<string> guidList = new();

            foreach (var availableMod in availableMods)
            {
                if (availableMod.loadedDefinition == null && loadedOnly) continue;
                if (onlineOnly && availableMod.OnlineRequirement != ModOnlineRequirement.RequiredByAllPlayers) continue;
                guidList.Add($"{availableMod.name} ({availableMod.identifier}) ({availableMod.GetGuid().ToString()})");
            }

            return guidList.ToArray();
        }
    }
}