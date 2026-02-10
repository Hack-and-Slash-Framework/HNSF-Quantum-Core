using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "HnSF/UMod/Content/Fighter Definitions Container", fileName = "fighterdefinitioncontainer")]
public class UModFighterDefinitionContainer : BaseFighterDefinitionContainer, IOnUModPrebuild
{
    [SerializeField] public ExternalModAssetSoftReference[] contentReferences;
    [SerializeField, HideInInspector] public ModAssetSoftReference[] fighterRefs;
    
    [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;

    public void OnUModPrebuild()
    {
        fighterRefs = new ModAssetSoftReference[contentReferences.Length];

        for (int i = 0; i < fighterRefs.Length; i++)
        {
            fighterRefs[i] = contentReferences[i].reference;
        }
    }
    
    public override UniTask<bool> Load(string id)
    {
        base.Load(id);
        definitionHandles = new LoadedAssetHandleWrapper[contentReferences.Length];
        return new UniTask<bool>(true);
    }

    public override async UniTask<bool> LoadAssets()
    {
        var modAsset = modDefinition.modAsset as UModModInfoAsset;
        
        try
        {
            Debug.Log($"FIGHTER REFERENCES {fighterRefs.Length}");
            for (var i = 0; i < fighterRefs.Length; i++)
            {
                var id = fighterRefs[i].assetID;
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError($"Couldn't get asset ID for {fighterRefs[i]} at index {i}.");
                    continue;
                }
                
                var loadResult = await modAsset.LoadAssetByIDAsync(fighterRefs[i].assetID);
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
            Debug.LogError($"Error in FighterDefinitionContainer {name} while loading fighters: {e}");
            return false;
        }

        return true;
    }

    public override IFighterDefinition[] GetFighters()
    {
        var fighterList = new List<IFighterDefinition>();
        if (definitionHandles == null) return fighterList.ToArray();

        foreach (var handle in definitionHandles)
        {
            fighterList.Add(handle.umodHandle.Result as IFighterDefinition);
        }
        return fighterList.ToArray();
    }

    public override void UnloadAssets()
    {
        if (definitionHandles == null) return;

        var modAsset = modDefinition.modAsset as UModModInfoAsset;
        
        for (int i = 0; i < definitionHandles.Length; i++)
        {
            modAsset.ReleaseAsset(definitionHandles[i]);
        }

        definitionHandles = null;
    }
}
