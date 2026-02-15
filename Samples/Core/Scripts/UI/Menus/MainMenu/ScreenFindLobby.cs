using HnSF.sessionhandling.handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class ScreenFindLobby : MenuBase
    {
        public TMP_InputField inputFieldDirectConnection;

        public ScrollRect scrollRectRoomList;

        public MainMenuScreenHandler mainMenuScreenHandler;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            attemptingConnection = false;
            inputFieldDirectConnection.text = "localhost";
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }

        private bool attemptingConnection;
        public virtual async void BUTTON_DirectConnect()
        {
            if (attemptingConnection) return;
            if (string.IsNullOrEmpty(inputFieldDirectConnection.text))
            {
                return;
            }

            attemptingConnection = true;
            if (!await mainMenuScreenHandler.AttemptCreateOrJoinRoom(inputFieldDirectConnection.text))
            {
                attemptingConnection = false;
            }
        }

        public virtual async void BUTTON_CreateRoom()
        {
            if (attemptingConnection) return;
            attemptingConnection = true;
            
            if (!await mainMenuScreenHandler.AttemptCreateOrJoinRoom(string.Empty))
            {
                attemptingConnection = false;
            }
        }
        
        public virtual void BUTTON_RefreshRoomList()
        {
            
        }
    }
}