using CT.MenuNav;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class PageMainMenu : MenuPage
    {
        public MainMenuHelper helper;
        public Button buttonQuickMatch;
        
        [Header("Pages")]
        public PageFindLobby pageFindLobby;
        public LocalMatchScreenHelper localMatchScreenHelper;
        public PageSettings pageSettings;
        
        public void BUTTON_QuickMatch()
        {
            /*
            var mHandler = (currentManager as MainMenuScreenManager);
            _ = mHandler.GoTo_QuickMatchScreen();*/
            //helper.quick
        }

        public void BUTTON_RoomMatch()
        {
            _ = currentManager.TryForwardPageAsync(pageFindLobby);
        }

        public async void BUTTON_LocalMatch()
        {
            var playerCount = await helper.TryInitializePlayers();
            if (playerCount == null) return;
            localMatchScreenHelper.Open(playerCount.Value);
        }

        public void BUTTON_TrainingMode()
        {
            /*
            var mHandler = (MenuHandler as MainMenuScreenHandler);
            mHandler.GoTo_TrainingModeScreen();*/
        }
        
        public void BUTTON_Settings()
        {
            //_ = currentManager.TryForwardPageAsync(pageSettings);
        }

        public void BUTTON_Quit()
        {
            Application.Quit();
        }
    }
}