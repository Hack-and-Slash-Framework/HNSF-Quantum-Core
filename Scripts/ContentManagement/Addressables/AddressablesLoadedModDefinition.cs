using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesLoadedModDefinition : LoadedModDefinition
{
    public AsyncOperationHandle<AddressablesModInfoAsset> modAssetHandle;
    public AsyncOperationHandle<IResourceLocator> resourceLocatorHandle;
    public IResourceLocator resourceLocator;

}
