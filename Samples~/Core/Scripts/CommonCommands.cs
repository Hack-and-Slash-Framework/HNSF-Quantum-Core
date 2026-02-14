using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using Quantum;
using UnityEngine;

namespace HnSF.commands
{
    public static partial class CommonCommands
    {
        public static async UniTaskVoid StartLocalMatch(List<List<ModAssetSoftReference>> selectedCharacters,
            List<TeamBitmask> selectedTeams,
            ModAssetSoftReference selectedGamemode, string gamemodeSettings, ModAssetSoftReference selectedMap,
            bool inputAssignment = true)
        {
            var drm = HnSFManagersContainer.instance;
            if (drm == null) return;

            if (inputAssignment)
            {
                bool? devicePickerResult = null;

                drm.devicePickerUtility.Open(selectedCharacters.Count, selectedCharacters.Count);
                drm.devicePickerUtility.OnPickerConfirm += dpu => { devicePickerResult = true; };
                drm.devicePickerUtility.OnPickerCancel += dpu => { devicePickerResult = false; };
                await UniTask.WaitUntil(() => devicePickerResult.HasValue);

                if (devicePickerResult == null || devicePickerResult.Value == false)
                {
                    drm.devicePickerUtility.Close();
                    return;
                }

                var validPlayers = drm.devicePickerUtility.GetValidInputPlayers();
                drm.inputManager.SetPlayersBasedOnDeviceLists(validPlayers);
                drm.inputManager.SwitchAllToUIActionMap();
                drm.devicePickerUtility.Close();
            }

            var gameManager = HnSFManagersContainer.instance;

            var localMatchSessionHandler = gameManager.sessionHandlerManager.CreateSessionHandler("LocalMatch",
                drm.sessionHandlerManager.localMatchSessionHandlerPrefab);
            if (localMatchSessionHandler == null) return;


            List<List<LoadedAssetHandleWrapper>> playerCharacters = new List<List<LoadedAssetHandleWrapper>>();

            bool characterLoadFailed = false;
            for (int i = 0; i < selectedCharacters.Count; i++)
            {
                var charaList = new List<LoadedAssetHandleWrapper>();

                for (int w = 0; w < selectedCharacters[i].Count; w++)
                {
                    var charaAssetHandle =
                        await gameManager.contentManager.LoadAssetFromModAsync(selectedCharacters[i][w]);
                    if (charaAssetHandle.result == false)
                    {
                        characterLoadFailed = true;
                        break;
                    }

                    charaList.Add(charaAssetHandle.handle);
                }

                if (characterLoadFailed) break;
                playerCharacters.Add(charaList);
            }

            if (characterLoadFailed)
            {
                foreach (var playerCharacterList in playerCharacters)
                {
                    foreach (var charaAssetHandle in playerCharacterList)
                    {
                        gameManager.contentManager.ReleaseAssetFromMod(charaAssetHandle);
                    }
                }

                playerCharacters.Clear();
                return;
            }

            var contentBundles =
                await PlayerMatchContentBundle.TryBuildPlayerContentBundles(playerCharacters, selectedTeams);
            localMatchSessionHandler.gamemodeConfiguration = gamemodeSettings;
            _ = localMatchSessionHandler.PrepareForMatchAndStart(new QuantumMatchContentBundle()
            {
                gamemodeReference = selectedGamemode,
                gamemodeSettings = "",
                mapReference = selectedMap,
                musicReference = default,
                localPlayerBundles = contentBundles,
                clientCount = 1,
                playerCount = selectedCharacters.Count
            });
        }
    }
}