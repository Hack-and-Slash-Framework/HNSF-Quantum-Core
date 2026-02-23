using System;
using HnSF.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class QuickMatchScreenInstance : MenuHandlerBase
    {
        public QuickMatchScreenHandler instanceHandler;
        
        [NonSerialized] public QuickMatchScreenHandler.QuickMatchLocalPlayerInfo playerInfo;

        [NonSerialized] public Camera instanceCamera;
        public InputPlayerManagerBase inputPlayer;

        public Canvas canvas;
        
        public Button buttonReadyUp;
        public Button buttonGamemode;
        public Button buttonCharacter1;

        public TMP_InputField usernameInputField;
        public TextMeshProUGUI modIdText;
        
        public QuickMatchScreenMainMenu screenMainMenu;
        [NonSerialized] public GenericContentPickerInstance screenContentPicker;
        
        public void Open()
        {
            inputPlayer.mpEventSystem.SetSelectedGameObject(null);
            ResetAndForwardTo(screenMainMenu);
        }

        public void Close()
        {
            
        }
    }
}