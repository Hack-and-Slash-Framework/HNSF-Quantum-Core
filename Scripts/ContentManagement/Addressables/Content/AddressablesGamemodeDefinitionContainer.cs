using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Gamemode Definitions Container",
        fileName = "gamemodedefinitioncontainer")]
    public class AddressablesGamemodeDefinitionContainer : BaseGamemodeDefinitionContainer
    {
        [SerializeField] private AssetReferenceT<AddressablesGamemodeDefinition>[] gamemodeReferences;

        [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            definitionHandles = new LoadedAssetHandleWrapper[gamemodeReferences.Length];
            return new UniTask<bool>(true);
        }

        public override async UniTask<bool> LoadGamemodeDefinitions()
        {
            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;

            try
            {
                for (var i = 0; i < gamemodeReferences.Length; i++)
                {
                    var id = await GeneralHelpers.GetAssetRefID(gamemodeReferences[i]);
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {gamemodeReferences[i]} at index {i}.");
                        continue;
                    }

                    var loadResult = await modAsset.LoadAssetByIDAsync(id);
                    if (!loadResult.result)
                    {
                        Debug.LogError($"Couldn't load asset ID {id} at index {i}. ({name})");
                        continue;
                    }

                    definitionHandles[i] = loadResult.handle;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in GamemodeDefinitionContainer {name} while loading gamemodes: {e}");
                return false;
            }

            return true;
        }

        public override BaseGamemodeDefinition[] GetGamemodes()
        {
            var contentList = new List<BaseGamemodeDefinition>();
            if (definitionHandles == null) return contentList.ToArray();

            foreach (var handle in definitionHandles)
            {
                if(!handle.IsValid()) continue;
                contentList.Add(handle.addressablesHandle.Result as BaseGamemodeDefinition);
            }

            return contentList.ToArray();
        }

        public override void UnloadGamemodeDefinitions()
        {
            if (definitionHandles == null) return;

            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;

            for (int i = 0; i < definitionHandles.Length; i++)
            {
                if(definitionHandles[i].IsValid() == false) continue;
                modAsset.ReleaseAsset(definitionHandles[i]);
            }

            definitionHandles = null;
        }

        public override void Unload()
        {
            UnloadGamemodeDefinitions();
        }
    }
}