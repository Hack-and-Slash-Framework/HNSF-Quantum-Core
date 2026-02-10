using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;

public static class SongContentHelper
{
    public static async UniTask<List<BaseSongDefinition>> GetAllSongDefinitions()
    {
        var contentList = new List<BaseSongDefinition>();
        var contentManager = HnSFManagersContainer.instance.contentManager;

        await contentManager.LoadAssetFromModsAsync("songdefinitioncontainer");

        var contentContainers = contentManager.GetAssetFromMods("songdefinitioncontainer");

        foreach (var contentContainer in contentContainers)
        {
            var container = contentContainer as BaseSongDefinitionContainer;
            bool r = await container.LoadDefinitions();
            if (!r) continue;
            contentList.AddRange(container.GetDefinitions());
        }
        
        return contentList;
    }

    public static void UnloadSongDefinitions(List<BaseSongDefinition> contentToUnload)
    {
        foreach (var content in contentToUnload)
        {
            Debug.Log($"{content.modDefinition.information.name} : {content.ID}");
        }
    }
}