using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class CSSCustomCharacterSelectWidgetViewItem : MonoBehaviour
    {
        public Image bgImage;
        public TextMeshProUGUI fighterNameTextObject;
        public int fighterIndex;
        
        public Color selectedColor;
        public Color normalColor;

        public virtual void Initialize()
        {
            bgImage.color = normalColor;
        }
        
        public virtual void OnSelected()
        {
            bgImage.color = selectedColor;
        }

        public virtual void OnDeselected()
        {
            bgImage.color = normalColor;
        }

        public virtual void OnSubmit()
        {
            
        }
    }
}