using CT.LocalInputManagement;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuScreenHandler : MenuHandlerBase
    {
        public ScreenMainMenu screenMainMenu;
        public ScreenSettings screenSettings;
        public ScreenFindLobby screenFindLobby;

        public QuickMatchScreenHandler quickMatchScreenHandler;
        public TraditionalLobbyScreenHandler traditionalLobbyScreenHandler;
        public LocalMatchScreenHandler localMatchScreenHandler;
        
        public GameObject menuCamera;
        
        public SessionHandlerTraditionalLobby traditionalLobbySessionHandlerPrefab;
        
        public void Awake()
        {
            quickMatchScreenHandler.gameObject.SetActive(false);
            
            screenMainMenu.TryClose(MenuDirection.BACKWARDS, true);
            screenSettings.TryClose(MenuDirection.BACKWARDS, true);
            screenFindLobby.TryClose(MenuDirection.BACKWARDS, true);
            
            ResetAndForwardTo(screenMainMenu);
            menuCamera.SetActive(true);
        }

        public async UniTask GoTo_QuickMatchScreen()
        {
            if (!await TryInitializePlayers()) return;

            Forward(null);
            menuCamera.SetActive(false);
            quickMatchScreenHandler.Open();
        }

        public async UniTask<bool> AttemptCreateOrJoinRoom(string address)
        {
            if (!await TryInitializePlayers()) return false;
            if (address == "localhost") address = "127.0.0.1";
            
            var sessionHandler =
                HnSFManagersContainer.instance.sessionHandlerManager.CreateSessionHandler("onlineroom", traditionalLobbySessionHandlerPrefab);
            if (sessionHandler == null) return false;

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
            
            Forward(null);
            menuCamera.SetActive(false);

            traditionalLobbyScreenHandler.roomSessionHandler = sessionHandler;
            traditionalLobbyScreenHandler.Open();
            return true;
        }

        public async UniTask<bool> TryInitializePlayers()
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
                return false;
            }
            
            var validPlayers = devicePicker.GetValidInputPlayers();
            var inputManager = InputManagerBase.instance as InputManagerUIM;
            inputManager.SetPlayersBasedOnDeviceLists(validPlayers);
            inputManager.SwitchAllToUIActionMap();
            devicePicker.Close();
            return true;
        }

        public async UniTaskVoid GoTo_LocalMatchScreen()
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
                return;
            }
            
            var validPlayers = devicePicker.GetValidInputPlayers();
            var inputManager = InputManagerBase.instance as InputManagerUIM;
            inputManager.SetPlayersBasedOnDeviceLists(validPlayers);
            inputManager.SwitchAllToUIActionMap();
            devicePicker.Close();

            localMatchScreenHandler.Open();
            Forward(null);
        }

        public void GoTo_TrainingModeScreen()
        {
            
        }
    }
}