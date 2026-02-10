using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    public class AddressablesLocalModLoader : BaseModLoader
    {
        public override int LoaderType => (int)KnownModLoaderTypes.ADDRESSABLES_LOCAL;

        public override async UniTask<LoadedModDefinition> TryLoadMod(ModManager modManager,
            AvailableModDefinition modDefinition)
        {
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
                Debug.LogError($"Loading local mod failed. Couldn't find Resource Locator.");
                return null;
            }

            localResourceLocator.Locate("modinfoasset", typeof(AddressablesModInfoAsset), out var locations);

            if (locations == null || locations.Count == 0)
            {
                Debug.LogError("Loading local mod failed. Couldn't find modinfoasset.");
                return null;
            }

            var handle = Addressables.LoadAssetAsync<AddressablesModInfoAsset>(locations.First());
            await handle;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Loading local mod failed! {handle.OperationException}");
                return null;
            }

            var lmd = new AddressablesLocalLoadedModDefinition()
            {
                information = modDefinition,
                modAsset = handle.Result,
                resourceLocator = localResourceLocator
            };

            ((AddressablesModInfoAsset)lmd.modAsset).ShouldRegisterQuantumAssets = false;
            lmd.modAsset.ModDefinition = lmd;
            lmd.modAsset.OnLoad();
            return lmd;
        }

        public override bool TryUnloadMod(ModManager modManager, LoadedModDefinition modLoadedDefinition)
        {
            return false;
        }
    }
}