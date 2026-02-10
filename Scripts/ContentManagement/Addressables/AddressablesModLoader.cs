using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace HnSF
{
    public class AddressablesModLoader : BaseModLoader
    {
        public override int LoaderType => (int)KnownModLoaderTypes.ADDRESSABLES;

        public override async UniTask<LoadedModDefinition> TryLoadMod(ModManager modManager,
            AvailableModDefinition modDefinition)
        {
            try
            {
                var handle = Addressables.LoadContentCatalogAsync(Path.Combine(modDefinition.path, "catalog.json"), false);
                await handle;
                //ResourceLocationMap loadResult = await handle as ResourceLocationMap;
                ResourceLocationMap loadResult = handle.Result as ResourceLocationMap;
                AddressablesModInfoAsset modInfoAsset = null;
                foreach (var key in loadResult.Keys)
                {
                    if (!typeof(AddressablesModInfoAsset).IsAssignableFrom(loadResult.Locations[key][0].ResourceType))
                        continue;
                    var modInfoAssetHandle =
                        Addressables.LoadAssetAsync<AddressablesModInfoAsset>(loadResult.Locations[key][0]);
                    await modInfoAssetHandle;
                    modInfoAsset = modInfoAssetHandle.Result;
                    break;
                }

                var lmd = new AddressablesLoadedModDefinition()
                {
                    information = modDefinition,
                    modAsset = modInfoAsset,
                    resourceLocator = loadResult,
                    resourceLocatorHandle = handle
                };
                return lmd;
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Error while loading {modDefinition.identifier} wtih {LoaderType.ToString()} loader: {e.ToString()}");
            }

            return null;
        }

        public override bool TryUnloadMod(ModManager modManager, LoadedModDefinition modLoadedDefinition)
        {
            if (modLoadedDefinition.information.canUnload == false) return false;

            var mld = modLoadedDefinition as AddressablesLoadedModDefinition;

            mld.resourceLocator = null;
            Addressables.Release(mld.resourceLocatorHandle);

            return true;
        }
    }
}
