using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;

public static class GamemodeContentHelper
{
    public static async UniTask<List<BaseGamemodeDefinition>> GetAllGamemodeDefinitions()
    {
        var gamemodeList = new List<BaseGamemodeDefinition>();
        var contentManager = HnSFManagersContainer.instance.contentManager;

        await contentManager.LoadAssetFromModsAsync("gamemodedefinitioncontainer");

        var gamemodeContainers = contentManager.GetAssetFromMods("gamemodedefinitioncontainer");

        foreach (var gamemodeContainer in gamemodeContainers)
        {
            var container = gamemodeContainer as BaseGamemodeDefinitionContainer;
            bool r = await container.LoadGamemodeDefinitions();
            if (!r) continue;
            gamemodeList.AddRange(container.GetGamemodes());
        }
        
        return gamemodeList;
    }

    public static void UnloadGamemodeDefinitions(List<BaseGamemodeDefinition> gamemodesToUnload)
    {
        foreach (var gamemode in gamemodesToUnload)
        {
            Debug.Log($"{gamemode.modDefinition.information.name} : {gamemode.ID}");
        }
    }
}
