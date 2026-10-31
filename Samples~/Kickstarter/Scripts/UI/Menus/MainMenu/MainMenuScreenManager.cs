using CT.MenuNav;
using HnSF.sessionhandling.handlers;
using HnSF.ui.menus.traditionallobby;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuScreenManager : MenuManager, IMenuInputOnConfirm, IMenuInputOnBack, IMenuInputOnStart, IMenuInputOnNavigate
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
                _ = page.TryCloseAsync(new MenuNavContext(MenuNavDirection.Back, isForced: true));
            
            
            _ = TryForwardPageAsync(mainMenuScreen.pageMainMenu);
        }

        private void Update()
        {
            if(Keyboard.current[Key.F2].wasPressedThisFrame) PrintBreadcrumbs();
        }

        public async void ReturnToLobbyScreen()
        {
            var sessionHandler = HnSFManagersContainer.instance.sessionHandlerManager.GetSessionHandler<SessionHandlerTraditionalLobby>("onlineroom");

            await TryForwardPageAsync(mainMenuScreen.pageMainMenu);
            lobbyScreen.roomSessionHandler = sessionHandler;
            await TryForwardPageAsync(lobbyScreen.pageLobbyMainMenu);
        }

        public void OnInputConfirmPressed(MenuInputButtonPhase buttonPhase, MenuInputContext context)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Current is not IMenuInputOnConfirm cMenu)
                return;
            cMenu.OnInputConfirmPressed(buttonPhase, context);
        }

        public void OnInputBackPressed(MenuInputButtonPhase buttonPhase, MenuInputContext context)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Current is not IMenuInputOnBack cMenu)
                return;
            cMenu.OnInputBackPressed(buttonPhase, context);
        }

        public void OnInputStartPressed(MenuInputButtonPhase buttonPhase, MenuInputContext context)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Current is not IMenuInputOnStart cMenu)
                return;
            cMenu.OnInputStartPressed(buttonPhase, context);
        }

        public void OnNavigate(Vector2 navInput, MenuInputContext context)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Current is not IMenuInputOnNavigate cMenu)
                return;
            cMenu.OnNavigate(navInput, context);
        }
        
        public void OnNavigateRaw(Vector2 navInput, MenuInputContext context)
        {
            if (Breadcrumb.Count == 0 || Breadcrumb.Current is not IMenuInputOnNavigateRaw cMenu)
                return;
            cMenu.OnNavigateRaw(navInput, context);
        }
    }
}
