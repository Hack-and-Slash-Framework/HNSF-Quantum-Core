using Cysharp.Threading.Tasks;

namespace HnSF
{
    public class BaseModLoader
    {
        public virtual int LoaderType => (int)KnownModLoaderTypes.ADDRESSABLES_LOCAL;

        public virtual async UniTask<LoadedModDefinition> TryLoadMod(ModManager modManager,
            AvailableModDefinition modDefinition)
        {
            await UniTask.WaitForFixedUpdate();
            return null;
        }

        public virtual bool TryUnloadMod(ModManager modManager, LoadedModDefinition modLoadedDefinition)
        {
            return false;
        }
    }
}