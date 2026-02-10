using TMPro;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenCreateRoomScreenMainMenu : MenuBase
    {
        public TMP_InputField roomNameInputField;
        public Button gamemodeButton;
        public Button gamemodeSettingsButton;
        public Button stageButton;
        public Button createRoomButton;
        public Button cancelButton;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            gamemodeSettingsButton.interactable = false;
            createRoomButton.interactable = false;
            CheckButtonInteractable();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }

        public void CheckButtonInteractable()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            gamemodeSettingsButton.interactable = handler.gamemodeHandle.IsValid();

            createRoomButton.interactable = handler.gamemodeHandle.IsValid() && handler.mapHandle.IsValid() && !string.IsNullOrEmpty(handler.gamemodeSettings);
        }
        
        public void BUTTON_Gamemode()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.OpenGamemodePickerScreen();
        }

        public void BUTTON_GamemodeSettings()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.OpenGamemodeConfigScreen();
        }
        
        public void BUTTON_Stage()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.OpenMapPickerScreen();
        }
        
        public void BUTTON_CreateRoom()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.AttemptCreateRoom();
        }
        
        public void BUTTON_Cancel()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.MenuHandler.Back();
        }
    }
}