using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TLCreateRoomPage : MenuPage
    {
        [Space]
        public TraditionalLobbyScreenHelper helper;
        
        public TMP_InputField roomNameInputField;
        public Button gamemodeButton;
        public Button gamemodeSettingsButton;
        public Button stageButton;
        public Button createRoomButton;
        public Button cancelButton;

        public override UniTask<bool> TryOpenAsync(MenuNavContext context)
        {
            gamemodeSettingsButton.interactable = false;
            createRoomButton.interactable = false;
            CheckButtonInteractable();
            return base.TryOpenAsync(context);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavContext context)
        {
            return base.TryCloseAsync(context);
        }
        
        public void CheckButtonInteractable()
        {
            gamemodeSettingsButton.interactable = helper.gamemodeHandle is { IsValid: true };

            createRoomButton.interactable = helper.gamemodeHandle is {IsValid: true} && helper.mapHandle is {IsValid: true} && !string.IsNullOrEmpty(helper.gamemodeSettings);
        }
        
        public async void BUTTON_Gamemode()
        {
            helper.screenContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<BaseGamemodeDefinition>(transform.parent);
            await helper.screenManager.TryForwardPageAsync(helper.screenContentPicking);
            helper.screenContentPicking.onContentPicked.AddListener(OnGamemodePicked);
            helper.screenContentPicking.onCancel.AddListener(OnGamemodePickCanceled);
            helper.screenContentPicking.Initialize<BaseGamemodeDefinition>();
        }
        
        private void OnGamemodePickCanceled(GenericContentPickerInstance arg0)
        {
            _ = helper.screenManager.TryBackPageAsync();
        }

        private void OnGamemodePicked(GenericContentPickerInstance arg0)
        {
            if (helper.gamemodeHandle is {IsValid: true})
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(helper.gamemodeHandle);
            }
            helper.gamemodeHandle = helper.screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            _ = helper.screenManager.TryBackPageAsync();
        }
        
        public void BUTTON_GamemodeSettings()
        {
            _ = helper.screenManager.TryForwardPageAsync(helper.gamemodeConfigPage);
        }
        
        public async void BUTTON_Stage()
        {
            helper.screenContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<IMapDefinition>(transform.parent);
            await helper.screenManager.TryForwardPageAsync(helper.screenContentPicking);
            helper.screenContentPicking.onContentPicked.AddListener(OnMapPicked);
            helper.screenContentPicking.onCancel.AddListener(OnMapPickCanceled);
            helper.screenContentPicking.Initialize<IMapDefinition>();
        }
        
        private void OnMapPicked(GenericContentPickerInstance arg0)
        {
            if (helper.mapHandle is {IsValid: true})
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(helper.mapHandle);
            }
            helper.mapHandle = helper.screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            _ = helper.screenManager.TryBackPageAsync();
        }

        private void OnMapPickCanceled(GenericContentPickerInstance arg0)
        {
            _ = helper.screenManager.TryBackPageAsync();
        }
        
        private bool creatingRoom = false;
        public async void BUTTON_CreateRoom()
        {
            if (creatingRoom) return;
            creatingRoom = true;
            
            var createResult = await helper.roomSessionHandler.CreateRoom(roomNameInputField.text, 2, 16, helper.gamemodeHandle, helper.gamemodeSettings, helper.mapHandle);
            creatingRoom = false;
            if (createResult == -1)
            {
                return;
            }
            
            await helper.screenManager.TryBackPageAsync();
            helper.GoTo_RoomScreen(createResult);
        }
        
        public void BUTTON_Cancel()
        {
            _ = helper.screenManager.TryBackPageAsync();
        }
    }
}
