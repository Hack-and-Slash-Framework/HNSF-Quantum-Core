using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UMod;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Hud Element Definitions Container", fileName = "hudelementdefinitioncontainer")]
    public class UModHudElementDefinitionContainer : BaseHudElementDefinitionContainer
    {
        [SerializeField] private ExternalModAssetSoftReference[] contentReferences;
    
        [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;
        
        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            definitionHandles = new LoadedAssetHandleWrapper[contentReferences.Length];
            return new UniTask<bool>(true);
        }
        
        public override async UniTask<bool> LoadDefinitions()
        {
            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;
        
            try
            {
                for (var i = 0; i < contentReferences.Length; i++)
                {
                    var id = contentReferences[i].reference.assetID;
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {contentReferences[i]} at index {i}.");
                        continue;
                    }
                    
                    var loadResult = await modAsset.LoadAssetByIDAsync<AddressablesHudElementDefinition>(id);
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
                Debug.LogError($"Error in {name} ({modAsset.ModDefinition.information.identifier}) while loading content definitions: {e}");
                return false;
            }

            return true;
        }

        public override BaseHudElementDefinition[] GetDefinitions()
        {
            var contentList = new List<BaseHudElementDefinition>();
            if (definitionHandles == null) return contentList.ToArray();

            foreach (var handle in definitionHandles)
            {
                if(!handle.IsValid()) continue;
                contentList.Add(handle.umodHandle.Result as BaseHudElementDefinition);
            }
            return contentList.ToArray();
        }

        public override void UnloadDefinitions()
        {
            if (definitionHandles == null) return;

            var modAsset = modDefinition.modAsset as UModModInfoAsset;
        
            for (int i = 0; i < definitionHandles.Length; i++)
            {
                if(definitionHandles[i].IsValid() == false) continue;
                modAsset.ReleaseAsset(definitionHandles[i]);
            }

            definitionHandles = null;
        }

        public override void Unload()
        {
            UnloadDefinitions();
        }
    }
}
