using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class QuickMatchScreenMainMenu : MenuBase
    {
        public QuickMatchScreenInstance quickMatchScreenInstance;
        
        public Canvas canvas;
        
        public Button buttonReadyUp;
        public Button buttonGamemode;
        public Button buttonCharacter1;

        public TMP_InputField usernameInputField;
        public TextMeshProUGUI modIdText;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            canvas.worldCamera = quickMatchScreenInstance.instanceCamera;
            gameObject.SetActive(true);
            usernameInputField.onEndEdit.AddListener(WhenUsernameSubmitted);
            UpdateReadyUpButtonState();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
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
            quickMatchScreenInstance.screenContentPicker.inputPlayer = quickMatchScreenInstance.inputPlayer;
            quickMatchScreenInstance.Forward(quickMatchScreenInstance.screenContentPicker);
            quickMatchScreenInstance.screenContentPicker.Initialize<IFighterDefinition>(quickMatchScreenInstance.inputPlayer);
            quickMatchScreenInstance.screenContentPicker.SetCameraTarget(quickMatchScreenInstance.instanceCamera);
            
            quickMatchScreenInstance.screenContentPicker.onContentPicked.AddListener(WhenCharacter1Submitted);
            quickMatchScreenInstance.screenContentPicker.onCancel.AddListener(WhenCancelPickingCharacter);
        }

        private void WhenCancelPickingCharacter(GenericContentPickerInstance arg0)
        {
            quickMatchScreenInstance.Back();
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
            quickMatchScreenInstance.Back();
        }

        public void BUTTON_Gamemode()
        {
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            quickMatchScreenInstance.screenContentPicker = contentPickerInstanceManager.CreateInstance<BaseGamemodeDefinition>(quickMatchScreenInstance.transform);
            quickMatchScreenInstance.screenContentPicker.inputPlayer = quickMatchScreenInstance.inputPlayer;
            quickMatchScreenInstance.Forward(quickMatchScreenInstance.screenContentPicker);
            quickMatchScreenInstance.screenContentPicker.Initialize<BaseGamemodeDefinition>(quickMatchScreenInstance.inputPlayer);
            quickMatchScreenInstance.screenContentPicker.SetCameraTarget(quickMatchScreenInstance.instanceCamera);
            
            quickMatchScreenInstance.screenContentPicker.onContentPicked.AddListener(WhenGamemodeSubmitted);
            quickMatchScreenInstance.screenContentPicker.onCancel.AddListener(WhenCancelPickingGamemode);
        }

        private void WhenCancelPickingGamemode(GenericContentPickerInstance arg0)
        {
            quickMatchScreenInstance.Back();
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
            
            quickMatchScreenInstance.Back();
        }

        public void BUTTON_ToggleReady()
        {
            quickMatchScreenInstance.playerInfo.SetReady(!quickMatchScreenInstance.playerInfo.Ready);
        }
    }
}