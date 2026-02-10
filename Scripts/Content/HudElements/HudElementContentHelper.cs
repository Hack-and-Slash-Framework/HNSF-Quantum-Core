using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;

public static class HudElementContentHelper
{
    public static async UniTask<List<BaseHudElementDefinition>> GetAllContentDefinitions()
    {
        var contentList = new List<BaseHudElementDefinition>();
        var contentManager = HnSFManagersContainer.instance.contentManager;

        await contentManager.LoadAssetFromModsAsync("hudelementdefinitioncontainer");

        var contentContainers = contentManager.GetAssetFromMods("hudelementdefinitioncontainer");

        foreach (var contentContainer in contentContainers)
        {
            var container = contentContainer as BaseHudElementDefinitionContainer;
            bool r = await container.LoadDefinitions();
            if (!r) continue;
            contentList.AddRange(container.GetDefinitions());
        }
        
        return contentList;
    }

    public static void UnloadContentDefinitions(List<BaseHudElementDefinition> contentToUnload)
    {
        foreach (var definition in contentToUnload)
        {
            Debug.Log($"{definition.modDefinition.information.name} : {definition.ID}");
        }
    }
}