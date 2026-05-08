using System;
using CT.MenuNav;
using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.examples.mainmenu;
using UnityEngine;
using UnityEngine.Serialization;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenHelper : MonoBehaviour
    {
        public MainMenuScreenManager screenManager;
        public Camera instanceCamera;

        public TraditionalLobbyScreenMainMenu pageLobbyMainMenu;
        public TLCreateRoomPage screenCreateRoom;
        public TraditionalLobbyScreenRoom screenRoom;
        
        [HideInInspector] public SessionHandlerTraditionalLobby roomSessionHandler;
        public TraditionalLobbyUIRepresentation lobbyRepresentation;
        
        [NonSerialized] public GenericContentPickerInstance screenContentPicker;

        public LoadedAssetHandleWrapper lastLoadedGamemodeAssetHandle;
        
        // Create Room Info
        [FormerlySerializedAs("screenGamemodeConfig")] public TLCreateRoomGamemodeConfigPage gamemodeConfigPage;
        [NonSerialized] public GenericContentPickerInstance screenContentPicking;
        
        public LoadedAssetHandleWrapper gamemodeHandle;
        public string gamemodeSettings;
        public LoadedAssetHandleWrapper mapHandle;

        protected int playerId;
        
        public virtual bool Open()
        {
            Debug.Log("Trying Lobby Screen");
            if (roomSessionHandler == null) return false;
            lobbyRepresentation = new TraditionalLobbyUIRepresentation();
            roomSessionHandler.SetUiLobbyRepresentation(lobbyRepresentation);
            
            instanceCamera.gameObject.SetActive(true);
            gameObject.SetActive(true);

            playerId = 1;

            //pageLobbyMainMenu.TryCloseAsync(MenuNavDirection.Back_FORCED);
            //screenRoom.TryCloseAsync(MenuNavDirection.Back_FORCED);
            //screenCreateRoom.TryCloseAsync(MenuNavDirection.Back_FORCED);
            
            _ = screenManager.TryForwardPage(pageLobbyMainMenu);
            Debug.Log("Entered Lobby Screen.");
            return true;
        }

        public virtual void Close()
        {
            instanceCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public async void GoTo_RoomScreen(int roomId)
        {
            screenRoom.roomId = roomId;
            var sResult = await screenManager.TryForwardPage(screenRoom);
            Debug.Log(sResult);
        }

        public int GetLocalPlayerIndex()
        {
            return playerId - 1;
        }
    }
}