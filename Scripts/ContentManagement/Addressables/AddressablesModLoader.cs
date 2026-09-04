using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    public class AddressablesModLoader : BaseModLoader
    {
        public override int LoaderType => (int)KnownModLoaderTypes.ADDRESSABLES;

        public override async UniTask<LoadedModDefinition> TryLoadMod(ModManager modManager,
            AvailableModDefinition modDefinition)
        {
            AsyncOperationHandle<IResourceLocator> catalogHandle = default;
            AsyncOperationHandle<AddressablesModInfoAsset> modInfoAssetHandle = default;
            ResourceLocationMap locator = null;
            bool successfullyLoaded = false;

            try
            {
                catalogHandle =
                    Addressables.LoadContentCatalogAsync(Path.Combine(modDefinition.path, "catalog.json"), false);
                await catalogHandle;
                if (!catalogHandle.IsValid() || catalogHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(catalogHandle);
                    catalogHandle = default;
                    return null;
                }
                
                locator = catalogHandle.Result as ResourceLocationMap;
                Addressables.AddResourceLocator(locator);
                AddressablesModInfoAsset modInfoAsset = null;
                modInfoAssetHandle = default;
                foreach (var key in locator.Keys)
                {
                    if (!typeof(AddressablesModInfoAsset).IsAssignableFrom(locator.Locations[key][0].ResourceType))
                        continue;
                    modInfoAssetHandle =
                        Addressables.LoadAssetAsync<AddressablesModInfoAsset>(locator.Locations[key][0]);
                    await modInfoAssetHandle;
                    if (!modInfoAssetHandle.IsValid() || modInfoAssetHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Addressables.Release(modInfoAssetHandle);
                        modInfoAssetHandle = default;
                        continue;
                    }

                    modInfoAsset = modInfoAssetHandle.Result;
                    break;
                }

                if (modInfoAsset == null)
                {
                    return null;
                }

                var lmd = new AddressablesLoadedModDefinition()
                {
                    information = modDefinition,
                    modAsset = modInfoAsset,
                    modAssetHandle = modInfoAssetHandle,
                    resourceLocator = locator,
                    resourceLocatorHandle = catalogHandle
                };

                lmd.modAsset.ModDefinition = lmd;
                lmd.modAsset.OnLoad();
                successfullyLoaded = true;
                return lmd;
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Error while loading {modDefinition.identifier} wtih {LoaderType.ToString()} loader: {e.ToString()}");
            }
            finally
            {
                if (!successfullyLoaded)
                {
                    if (modInfoAssetHandle.IsValid())
                        Addressables.Release(modInfoAssetHandle);

                    if (locator != null)
                        Addressables.RemoveResourceLocator(locator);

                    if (catalogHandle.IsValid())
                        Addressables.Release(catalogHandle);
                }
            }
            
            return null;
        }

        public override async UniTask<bool> TryUnloadMod(ModManager modManager,
            LoadedModDefinition modLoadedDefinition)
        {
            if (modLoadedDefinition.information.canUnload == false) return false;

            if (modLoadedDefinition is not AddressablesLoadedModDefinition mld)
                return false;

            if (mld.modAsset is AddressablesModInfoAsset modAsset)
            {
                await modAsset.PrepareForUnloadAsync();
            }
            
            if(mld.modAssetHandle.IsValid())
                Addressables.Release(mld.modAssetHandle);
            mld.modAssetHandle = default;
            mld.modAsset = null;

            if (mld.resourceLocator != null)
                Addressables.RemoveResourceLocator(mld.resourceLocator);
            mld.resourceLocator = null;

            if(mld.resourceLocatorHandle.IsValid())
                Addressables.Release(mld.resourceLocatorHandle);
            mld.resourceLocatorHandle = default;

            return true;
        }
    }
}
