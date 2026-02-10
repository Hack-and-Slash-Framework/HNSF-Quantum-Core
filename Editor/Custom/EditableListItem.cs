using UnityEditor;
using UnityEngine.UIElements;

// Source: https://github.com/aarthificial-unity/typewriter
namespace HnSF
{
    public abstract class EditableListItem : VisualElement
    {
        public abstract void BindProperty(SerializedProperty property);
        public abstract void Unbind();
    }
}