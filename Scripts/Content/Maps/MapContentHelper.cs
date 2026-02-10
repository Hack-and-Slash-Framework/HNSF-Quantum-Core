using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;

public static class MapContentHelper
{
    public static async UniTask<List<IMapDefinition>> GetAllMapDefinitions()
    {
        var gamemodeList = new List<IMapDefinition>();
        var contentManager = HnSFManagersContainer.instance.contentManager;

        await contentManager.LoadAssetFromModsAsync("mapdefinitioncontainer");

        var mapContainers = contentManager.GetAssetFromMods("mapdefinitioncontainer");

        foreach (var mapContainer in mapContainers)
        {
            var container = mapContainer as BaseMapDefinitionContainer;
            bool r = await container.LoadMapDefinitions();
            if (!r) continue;
            gamemodeList.AddRange(container.GetMaps());
        }
        
        return gamemodeList;
    }

    public static void UnloadMapDefinitions(List<IMapDefinition> mapsToUnload)
    {
        foreach (var map in mapsToUnload)
        {
            Debug.Log($"{map.modDefinition.information.name} : {map.ID}");
        }
    }
}