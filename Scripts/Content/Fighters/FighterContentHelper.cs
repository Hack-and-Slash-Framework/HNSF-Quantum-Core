using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF;
using UnityEngine;

public static class FighterContentHelper
{
    public static async UniTask<List<IFighterDefinition>> GetAllFighterDefinitions()
    {
        var fighterList = new List<IFighterDefinition>();
        var contentManager = HnSFManagersContainer.instance.contentManager;

        await contentManager.LoadAssetFromModsAsync("fighterdefinitioncontainer");

        var fighterContainers = contentManager.GetAssetFromMods("fighterdefinitioncontainer");

        foreach (var fighterContainer in fighterContainers)
        {
            var container = fighterContainer as BaseFighterDefinitionContainer;
            bool r = await container.LoadAssets();
            if (!r) continue;
            fighterList.AddRange(container.GetFighters());
        }
        
        return fighterList;
    }

    public static void UnloadDefinitions(List<IFighterDefinition> fightersToUnload)
    {
        foreach (var fighter in fightersToUnload)
        {
            Debug.Log($"{fighter.modDefinition.information.name} : {fighter.ID}");
        }
    }
}