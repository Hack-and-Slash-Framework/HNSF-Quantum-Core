using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class ScreenMainMenu : MenuBase
    {
        public Button buttonQuickMatch;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }
        
        
        public void BUTTON_QuickMatch()
        {
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            _ = mHandler.GoTo_QuickMatchScreen();
        }

        public void BUTTON_RoomMatch()
        {
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            mHandler.Forward(mHandler.screenFindLobby);
        }

        public void BUTTON_LocalMatch()
        {
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            _ = mHandler.GoTo_LocalMatchScreen();
        }

        public void BUTTON_TrainingMode()
        {
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            mHandler.GoTo_TrainingModeScreen();
        }
        
        public void BUTTON_Settings()
        {
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            mHandler.Forward(mHandler.screenSettings);
        }

        public void BUTTON_Quit()
        {
            Application.Quit();
        }
    }
}