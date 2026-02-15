using System;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenCreateRoom : MenuHandlerAndMenuBase
    {
        
        public TraditionalLobbyScreenCreateRoomScreenMainMenu screenMainMenu;
        public TraditionalLobbyScreenCreateRoomScreenGamemodeConfig screenGamemodeConfig;
        [NonSerialized] public GenericContentPickerInstance screenContentPicking;

        public LoadedAssetHandleWrapper gamemodeHandle;
        public string gamemodeSettings;
        public LoadedAssetHandleWrapper mapHandle;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            ResetAndForwardTo(screenMainMenu);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }

        private bool creatingRoom = false;
        public async void AttemptCreateRoom()
        {
            if (creatingRoom) return;
            creatingRoom = true;
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            var createResult = await instanceHandler.roomSessionHandler.CreateRoom(screenMainMenu.roomNameInputField.text, 2, 16, gamemodeHandle, gamemodeSettings, mapHandle);
            creatingRoom = false;
            if (createResult == -1)
            {
                return;
            }

            MenuHandler.Back();
            (MenuHandler as TraditionalLobbyScreenHandler).GoTo_RoomScreen(createResult);
        }

        public void OpenGamemodeConfigScreen()
        {
            Forward(screenGamemodeConfig);
        }
        
        public void OpenGamemodePickerScreen()
        {
            screenContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<BaseGamemodeDefinition>(transform);
            screenContentPicking.onContentPicked.AddListener(OnGamemodePicked);
            screenContentPicking.onCancel.AddListener(OnGamemodePickCanceled);
            screenContentPicking.Initialize<BaseGamemodeDefinition>((MenuHandler as TraditionalLobbyScreenHandler).inputPlayer);
            Forward(screenContentPicking);
        }

        private void OnGamemodePickCanceled(GenericContentPickerInstance arg0)
        {
            Back();
        }

        private void OnGamemodePicked(GenericContentPickerInstance arg0)
        {
            if (gamemodeHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(gamemodeHandle);
            }
            gamemodeHandle = screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            Back();
        }
        
        public void OpenMapPickerScreen()
        {
            screenContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<IMapDefinition>(transform);
            screenContentPicking.inputPlayer = (MenuHandler as TraditionalLobbyScreenHandler).inputPlayer;
            screenContentPicking.onContentPicked.AddListener(OnMapPicked);
            screenContentPicking.onCancel.AddListener(OnMapPickCanceled);
            screenContentPicking.Initialize<IMapDefinition>((MenuHandler as TraditionalLobbyScreenHandler).inputPlayer);
            Forward(screenContentPicking);
        }

        private void OnMapPicked(GenericContentPickerInstance arg0)
        {
            if (mapHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(mapHandle);
            }
            mapHandle = screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            Back();
        }

        private void OnMapPickCanceled(GenericContentPickerInstance arg0)
        {
            Back();
        }
    }
}