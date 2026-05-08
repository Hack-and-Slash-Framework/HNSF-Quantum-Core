using CT.MenuNav;
using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuScreenManager : MenuManager, IMenuInputOnPressedConfirm, IMenuInputOnPressedBack, IMenuInputOnPressedStart, IMenuInputOnNavigate
    {
        public MainMenuHelper mainMenuScreen;
        public QuickMatchScreenHandler quickMatchScreen;
        public TraditionalLobbyScreenHelper lobbyScreen;
        public LocalMatchScreenHelper localMatchScreen;
        public PageSettings settingsPage;
        
        protected override async void Awake()
        {
            foreach(var cc in GetComponentsInChildren<MenuPage>())
                assignedPages.Add(cc);

            foreach (var page in assignedPages)
                _ = page.TryCloseAsync(MenuNavDirection.Back_FORCED);
            
            
            _ = TryForwardPage(mainMenuScreen.pageMainMenu);
        }

        private void Update()
        {
            if(Keyboard.current[Key.F2].wasPressedThisFrame) PrintBreadcrumbs();
        }

        public async void ReturnToLobbyScreen()
        {
            var sessionHandler = HnSFManagersContainer.instance.sessionHandlerManager.sessionHandlers["onlineroom"];

            await TryForwardPage(mainMenuScreen.pageMainMenu);
            lobbyScreen.roomSessionHandler = sessionHandler as SessionHandlerTraditionalLobby;
            await TryForwardPage(lobbyScreen.pageLobbyMainMenu);
        }

        public void OnInputConfirmPressed(int playerID, BaseEventData eventData)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Peek() is not IMenuInputOnPressedConfirm cMenu)
                return;
            cMenu.OnInputConfirmPressed(playerID, eventData);
        }

        public void OnInputBackPressed(int playerID, BaseEventData eventData)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Peek() is not IMenuInputOnPressedBack cMenu)
                return;
            cMenu.OnInputBackPressed(playerID, eventData);
        }

        public void OnInputStartPressed(int playerID, BaseEventData eventData)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Peek() is not IMenuInputOnPressedStart cMenu)
                return;
            cMenu.OnInputStartPressed(playerID, eventData);
        }

        public void OnNavigate(Vector2 navInput, int playerID, BaseEventData eventData)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Peek() is not IMenuInputOnNavigate cMenu)
                return;
            cMenu.OnNavigate(navInput, playerID, eventData);
        }
        
        public void OnNavigateRaw(Vector2 navInput, int playerID, BaseEventData eventData)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Peek() is not IMenuInputOnNavigateRaw cMenu)
                return;
            cMenu.OnNavigateRaw(navInput, playerID, eventData);
        }
    }
}