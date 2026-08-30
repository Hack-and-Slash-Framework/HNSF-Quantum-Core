using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class QuickMatchScreenMainMenu : MenuPage
    {
        public QuickMatchScreenInstance quickMatchScreenInstance;
        
        public Canvas canvas;
        
        public Button buttonReadyUp;
        public Button buttonGamemode;
        public Button buttonCharacter1;

        public TMP_InputField usernameInputField;
        public TextMeshProUGUI modIdText;

        public override UniTask<bool> TryOpenAsync(MenuNavContext context)
        {
            canvas.worldCamera = quickMatchScreenInstance.instanceCamera;
            usernameInputField.onEndEdit.AddListener(WhenUsernameSubmitted);
            UpdateReadyUpButtonState();
            return base.TryOpenAsync(context);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavContext context)
        {
            return base.TryCloseAsync(context);
        }
        
        private void UpdateReadyUpButtonState()
        {
            buttonReadyUp.interactable = false;

            if (quickMatchScreenInstance.playerInfo.assetHandleCharacter.IsValid() == false
                || quickMatchScreenInstance.instanceHandler.selectedGamemodeDefinition.IsValid() == false)
            {
                return;
            }

            buttonReadyUp.interactable = true;
        }
        
        private void WhenUsernameSubmitted(string arg0)
        {
            quickMatchScreenInstance.playerInfo.PlayerName = arg0;
        }
        
        public void BUTTON_SetCharacter(int index)
        {
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            quickMatchScreenInstance.screenContentPicker = contentPickerInstanceManager.CreateInstance<IFighterDefinition>(quickMatchScreenInstance.transform);
            _ = quickMatchScreenInstance.TryForwardPageAsync(quickMatchScreenInstance.screenContentPicker);
            quickMatchScreenInstance.screenContentPicker.Initialize<IFighterDefinition>();
            quickMatchScreenInstance.screenContentPicker.SetCameraTarget(quickMatchScreenInstance.instanceCamera);
            
            quickMatchScreenInstance.screenContentPicker.onContentPicked.AddListener(WhenCharacter1Submitted);
            quickMatchScreenInstance.screenContentPicker.onCancel.AddListener(WhenCancelPickingCharacter);
        }

        private void WhenCancelPickingCharacter(GenericContentPickerInstance arg0)
        {
            _ = quickMatchScreenInstance.TryBackPageAsync();
            GameObject.Destroy(quickMatchScreenInstance.screenContentPicker);
        }

        private void WhenCharacter1Submitted(GenericContentPickerInstance arg0)
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var characterAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            if (quickMatchScreenInstance.playerInfo.assetHandleCharacter.IsValid())
            {
                contentManager.ReleaseAssetFromMod(quickMatchScreenInstance.playerInfo.assetHandleCharacter);
            }
            quickMatchScreenInstance.playerInfo.SetCharacterAssetHandle(characterAssetHandle);
            _ = quickMatchScreenInstance.TryBackPageAsync();
        }

        public void BUTTON_Gamemode()
        {
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            quickMatchScreenInstance.screenContentPicker = contentPickerInstanceManager.CreateInstance<BaseGamemodeDefinition>(quickMatchScreenInstance.transform);
            _ = quickMatchScreenInstance.TryForwardPageAsync(quickMatchScreenInstance.screenContentPicker);
            quickMatchScreenInstance.screenContentPicker.Initialize<BaseGamemodeDefinition>();
            quickMatchScreenInstance.screenContentPicker.SetCameraTarget(quickMatchScreenInstance.instanceCamera);
            
            quickMatchScreenInstance.screenContentPicker.onContentPicked.AddListener(WhenGamemodeSubmitted);
            quickMatchScreenInstance.screenContentPicker.onCancel.AddListener(WhenCancelPickingGamemode);
        }

        private void WhenCancelPickingGamemode(GenericContentPickerInstance arg0)
        {
            _ = quickMatchScreenInstance.TryBackPageAsync();
            GameObject.Destroy(quickMatchScreenInstance.screenContentPicker);
        }

        private void WhenGamemodeSubmitted(GenericContentPickerInstance arg0)
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var gamemodeAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            if (quickMatchScreenInstance.instanceHandler.selectedGamemodeDefinition.IsValid())
            {
                contentManager.ReleaseAssetFromMod(quickMatchScreenInstance.instanceHandler.selectedGamemodeDefinition);
            }

            quickMatchScreenInstance.instanceHandler.selectedGamemodeDefinition = gamemodeAssetHandle;

            _ = quickMatchScreenInstance.TryBackPageAsync();
        }

        public void BUTTON_ToggleReady()
        {
            quickMatchScreenInstance.playerInfo.SetReady(!quickMatchScreenInstance.playerInfo.Ready);
        }
    }
}
