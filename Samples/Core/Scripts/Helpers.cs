using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF.BaseExample
{
    public static class Helpers
    {
        public static async UniTask<bool> TrySetupLocalPlayers()
        {
            var gameManager = HnSFManagersContainer.instance;
            bool ss = false;
            bool pResult = true;
            var inputManager = gameManager.inputManager;
            var devicePicker = DevicePickerUtility.instance;
            devicePicker.Open(minimumPlayers: 1, maximumPlayers: 4);
            devicePicker.OnPickerCancel += (dpu) =>
            {
                ss = true;
                pResult = false;
            };
            devicePicker.OnPickerConfirm += (dpu) => ss = true;
            await UniTask.WaitUntil(() => ss == true);

            if (pResult == false) return false;

            var players = devicePicker.GetValidInputPlayers();
            devicePicker.Close();

            gameManager.inputManager.SetPlayerCount(players.Count);
            inputManager.ReturnAllDevicesToSystem();
            for (int i = 0; i < players.Count; i++)
            {
                inputManager.AssignDevicesToPlayer(players[i].ToArray(), i + 1);
                inputManager.playerInputManagers[i + 1].SwitchToPlayerMap();
            }

            return true;
        }
    }
}