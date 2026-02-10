using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuScreenResetter : MonoBehaviour
    {
        public MainMenuScreenHandler mainMenuScreen;
        public QuickMatchScreenHandler quickMatchScreen;
        public TraditionalLobbyScreenHandler lobbyScreen;
        public LocalMatchScreenHandler localMatchScreen;
        
        private void Awake()
        {
            quickMatchScreen.Close();
            lobbyScreen.Close();
            localMatchScreen.Close();
        }

        public void ReturnToLobbyScreen()
        {
            var sessionHandler = HnSFManagersContainer.instance.sessionHandlerManager.sessionHandlers["onlineroom"];

            mainMenuScreen.Forward(null);
            lobbyScreen.roomSessionHandler = sessionHandler as SessionHandlerTraditionalLobby;
            lobbyScreen.Open();
        }
    }
}