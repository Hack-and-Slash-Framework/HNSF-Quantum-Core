using System;
using HnSF.Input;
using HnSF.sessionhandling.handlers;
using UnityEngine;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenHandler : MenuHandlerBase
    {
        [HideInInspector] public InputPlayerManager inputPlayer;
        public Camera instanceCamera;

        public TraditionalLobbyScreenMainMenu screenMainMenu;
        public TraditionalLobbyScreenCreateRoom screenCreateRoom;
        public TraditionalLobbyScreenRoom screenRoom;
        
        [HideInInspector] public SessionHandlerTraditionalLobby roomSessionHandler;
        public TraditionalLobbyUIRepresentation lobbyRepresentation;
        
        [NonSerialized] public GenericContentPickerInstance screenContentPicker;

        public LoadedAssetHandleWrapper lastLoadedGamemodeAssetHandle;
        
        public bool Open()
        {
            if (roomSessionHandler == null) return false;
            inputPlayer = HnSFManagersContainer.instance.inputManager.GetPlayer(1);
            lobbyRepresentation = new TraditionalLobbyUIRepresentation();
            roomSessionHandler.SetUiLobbyRepresentation(lobbyRepresentation);
            
            instanceCamera.gameObject.SetActive(true);
            gameObject.SetActive(true);

            screenRoom.TryClose(MenuDirection.BACKWARDS, true);
            screenCreateRoom.TryClose(MenuDirection.BACKWARDS, true);
            screenMainMenu.TryClose(MenuDirection.BACKWARDS, true);
            
            ResetAndForwardTo(screenMainMenu);
            return true;
        }

        public void Close()
        {
            instanceCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void GoTo_RoomScreen(int roomId)
        {
            screenRoom.roomId = roomId;
            Forward(screenRoom);
        }

        public int GetLocalPlayerID()
        {
            return inputPlayer.Id - 1;
        }
    }
}