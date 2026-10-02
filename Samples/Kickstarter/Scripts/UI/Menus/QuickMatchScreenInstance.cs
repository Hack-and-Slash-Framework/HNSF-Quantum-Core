using System;
using CT.MenuNav;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class QuickMatchScreenInstance : MenuManager
    {
        public QuickMatchScreenHandler instanceHandler;
        
        [NonSerialized] public QuickMatchScreenHandler.QuickMatchLocalPlayerInfo playerInfo;

        [NonSerialized] public Camera instanceCamera;
        
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
            SetCurrentSelectedGameObject(null, 1);
            _ = TryForwardPageAsync(screenMainMenu);
        }

        public void Close()
        {
            
        }
    }
}
