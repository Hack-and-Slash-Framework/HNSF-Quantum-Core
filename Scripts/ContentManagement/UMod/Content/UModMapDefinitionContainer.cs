using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Map Definitions Container", fileName = "mapdefinitioncontainer")]
    public class UModMapDefinitionContainer : BaseMapDefinitionContainer
    {
        [SerializeField] private ExternalModAssetSoftReference[] mapReferences;

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
                    var id = mapReferences[i].reference.assetID;
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {mapReferences[i]} at index {i}.");
                        continue;
                    }

                    var loadResult = await modAsset.LoadAssetByIDAsync<UModMapDefinition>(id);
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
                contentList.Add(handle.umodHandle.Result as IMapDefinition);
            }

            return contentList.ToArray();
        }

        public override void UnloadMapDefinitions()
        {
            if (definitionHandles == null) return;

            var modAsset = modDefinition.modAsset as UModModInfoAsset;

            for (int i = 0; i < definitionHandles.Length; i++)
            {
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