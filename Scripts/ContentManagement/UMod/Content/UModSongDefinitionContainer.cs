using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Song Definitions Container", fileName = "songdefinitioncontainer")]
    public class UModSongDefinitionContainer : BaseSongDefinitionContainer, IOnUModPrebuild
    {
        [SerializeField] private ExternalModAssetSoftReference[] contentReferences;
        [SerializeField, HideInInspector] public ModAssetSoftReference[] contentRefs;
        
        [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;
        
        public void OnUModPrebuild()
        {
            contentRefs = new ModAssetSoftReference[contentReferences.Length];

            for (int i = 0; i < contentRefs.Length; i++)
            {
                contentRefs[i] = contentReferences[i].reference;
            }
        }
        
        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            definitionHandles = new LoadedAssetHandleWrapper[contentReferences.Length];
            return new UniTask<bool>(true);
        }
        
        public override async UniTask<bool> LoadDefinitions()
        {
            var modAsset = modDefinition.modAsset as UModModInfoAsset;
        
            try
            {
                for (var i = 0; i < contentRefs.Length; i++)
                {
                    var id = contentRefs[i].assetID;
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {contentRefs[i]} at index {i}.");
                        continue;
                    }
                    
                    var loadResult = await modAsset.LoadAssetByIDAsync<UModSongDefinition>(id);
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

        public override BaseSongDefinition[] GetDefinitions()
        {
            var contentList = new List<BaseSongDefinition>();
            if (definitionHandles == null) return contentList.ToArray();

            foreach (var handle in definitionHandles)
            {
                contentList.Add(handle.umodHandle.Result as BaseSongDefinition);
            }
            return contentList.ToArray();
        }

        public override void UnloadDefinitions()
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
            UnloadDefinitions();
        }
    }
}
