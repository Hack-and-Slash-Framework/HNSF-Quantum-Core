using CT.LocalInputManagement;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;
using UnityEngine.Serialization;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuHelper : MonoBehaviour
    {
        [FormerlySerializedAs("screenMainMenu")] public PageMainMenu pageMainMenu;

        public QuickMatchScreenHandler quickMatchScreenHandler;
        [FormerlySerializedAs("traditionalLobbyScreenHandler")] public TraditionalLobbyScreenHelper traditionalLobbyScreenHelper;
        [FormerlySerializedAs("localMatchScreenHandler")] public LocalMatchScreenHelper localMatchScreenHelper;
        
        public GameObject menuCamera;
        
        public SessionHandlerTraditionalLobby traditionalLobbySessionHandlerPrefab;
        
        public void Awake()
        {
            menuCamera.SetActive(true);
        }
        
        public async UniTask<bool> AttemptCreateOrJoinRoom(string address)
        {
            var playerCount = await TryInitializePlayers();
            if (playerCount == null) return false;
            if (address == "localhost") address = "127.0.0.1";
            
            var sessionHandler =
                HnSFManagersContainer.instance.sessionHandlerManager.CreateSessionHandler("onlineroom", traditionalLobbySessionHandlerPrefab);
            if (sessionHandler == null) return false;

            sessionHandler.SetLocalPlayerCount(playerCount.Value);
            
            if (string.IsNullOrEmpty(address))
            {
                var connectedToRoom = await sessionHandler.TryCreateLobby();
                if (connectedToRoom == false)
                {
                    GameObject.Destroy(sessionHandler.gameObject);
                    return false;
                }
            }
            else
            {
                var connectedToRoom = await sessionHandler.TryJoinLobby(address);
                if (connectedToRoom == false)
                {
                    GameObject.Destroy(sessionHandler.gameObject);
                    return false;
                }
            }
            
            //menuCamera.SetActive(false);
            
            traditionalLobbyScreenHelper.roomSessionHandler = sessionHandler;
            traditionalLobbyScreenHelper.Open();
            return true;
        }

        public async UniTask<int?> TryInitializePlayers()
        {
            bool? devicePickerResult = null;
            
            var drm = HnSFManagersContainer.instance;
            var devicePicker = DevicePickerUtility.instance;
            devicePicker.Open(1, 4);
            devicePicker.OnPickerConfirm += dpu => { devicePickerResult = true; };
            devicePicker.OnPickerCancel += dpu => { devicePickerResult = false; };
            await UniTask.WaitUntil(() => devicePickerResult.HasValue);

            if (devicePickerResult == null || devicePickerResult.Value == false)
            {
                devicePicker.Close();
                return null;
            }
            
            var validPlayers = devicePicker.GetValidInputPlayers();
            var inputManager = InputManager.instance as InputManager;
            inputManager.SetPlayersBasedOnDeviceLists(validPlayers);
            inputManager.SwitchAllToUIActionMap();
            devicePicker.Close();
            return inputManager.GetPlayerCount();
        }
    }
}