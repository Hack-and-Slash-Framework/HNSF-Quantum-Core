using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Quantum;
using UMod;
using UnityEngine;

namespace HnSF
{
    public class UModModLoader : BaseModLoader
    {
        public override int LoaderType => (int)KnownModLoaderTypes.UMOD;

        public override UniTask<LoadedModDefinition> TryLoadMod(ModManager modManager,
            AvailableModDefinition modDefinition)
        {
            ModHost modHost = null;
            try
            {
                string[] wantedFile = System.IO.Directory.GetFiles(modDefinition.path, "*.umod");
                if (wantedFile == null || wantedFile.Length == 0) throw new Exception("No umod file found.");

                modHost = Mod.Load(new Uri(Path.Combine(modDefinition.path, wantedFile[0])));
                if (modHost.IsModLoaded == false)
                    throw new Exception($"UMod mod failed to load: {modHost.LoadResult.Error}");

                var mao = modHost.Assets.Load<UModModInfoAsset>("modinfoasset");

                var lmd = new UModLoadedModDefinition()
                {
                    information = modDefinition,
                    modAsset = mao as UModModInfoAsset,
                    modHost = modHost
                };
                mao.ModDefinition = lmd;
                mao.OnLoad();
                return new UniTask<LoadedModDefinition>(lmd);

            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Failed to load UMod mod {modHost} {modDefinition.identifier} {modDefinition.path}: {e.Message}");
                if (modHost.IsModLoaded) modHost.UnloadMod();
                // TODO: Check if dependencies were loaded and unload them.
            }

            return new UniTask<LoadedModDefinition>(null);
        }

        public override bool TryUnloadMod(ModManager modManager, LoadedModDefinition modLoadedDefinition)
        {
            var lmd = modLoadedDefinition as UModLoadedModDefinition;
            (lmd.modAsset as UModModInfoAsset).OnUnload();
            lmd.modHost.UnloadMod(true);
            return true;
        }
    }
}