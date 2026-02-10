using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Map Definitions Container",
        fileName = "mapdefinitioncontainer")]
    public class AddressablesMapDefinitionContainer : BaseMapDefinitionContainer
    {
        [SerializeField] private AssetReferenceT<AddressablesMapDefinition>[] mapReferences;

        [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            definitionHandles = new LoadedAssetHandleWrapper[mapReferences.Length];
            return new UniTask<bool>(true);
        }

        public override async UniTask<bool> LoadMapDefinitions()
        {
            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;

            try
            {
                for (var i = 0; i < mapReferences.Length; i++)
                {
                    var id = await GeneralHelpers.GetAssetRefID(mapReferences[i]);
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {mapReferences[i]} at index {i}.");
                        continue;
                    }

                    var loadResult = await modAsset.LoadAssetByIDAsync<AddressablesMapDefinition>(id);
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
                Debug.LogError($"Error in MapDefinitionContainer {name} while loading maps: {e}");
                return false;
            }

            return true;
        }

        public override IMapDefinition[] GetMaps()
        {
            var contentList = new List<IMapDefinition>();
            if (definitionHandles == null) return contentList.ToArray();

            foreach (var handle in definitionHandles)
            {
                if(!handle.IsValid()) continue;
                contentList.Add(handle.addressablesHandle.Result as IMapDefinition);
            }

            return contentList.ToArray();
        }

        public override void UnloadMapDefinitions()
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
            UnloadMapDefinitions();
        }
    }
}