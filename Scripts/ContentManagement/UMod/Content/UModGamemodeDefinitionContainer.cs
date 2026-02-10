using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "HnSF/UMod/Content/Gamemode Definitions Container", fileName = "gamemodedefinitioncontainer")]
public class UModGamemodeDefinitionContainer : BaseGamemodeDefinitionContainer
{
    [SerializeField] private ExternalModAssetSoftReference[] contentReferences;
    
    [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;

    public override UniTask<bool> Load(string id)
    {
        base.Load(id);
        definitionHandles = new LoadedAssetHandleWrapper[contentReferences.Length];
        return new UniTask<bool>(true);
    }

    public override async UniTask<bool> LoadGamemodeDefinitions()
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
                
                var loadResult = await modAsset.LoadAssetByIDAsync(contentReferences[i].reference.assetID);
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
            contentList.Add(handle.umodHandle.Result as BaseGamemodeDefinition);
        }
        return contentList.ToArray();
    }

    public override void UnloadGamemodeDefinitions()
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
        UnloadGamemodeDefinitions();
    }
}
