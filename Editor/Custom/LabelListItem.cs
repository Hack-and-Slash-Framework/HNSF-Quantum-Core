using HnSF.core.state;
using Quantum;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Source: https://github.com/aarthificial-unity/typewriter
namespace HnSF
{
    public class LabelListItem : EditableListItem
    {
        public readonly Label Label;
        public readonly VisualElement Root;
        public readonly TextField Text;
        public readonly Label Type;

        public string emptyText = "<empty>";

        public LabelListItem()
        {
            Root = new VisualElement();
            Root.AddToClassList("editable-item");

            Label = new Label();
            Label.AddToClassList("editable-item__label");
            Text = new TextField { style = { display = DisplayStyle.None } };
            Text.AddToClassList("editable-item__field");

            Type = new Label();
            Type.AddToClassList("editable-item__type");

            Label.RegisterCallback<MouseDownEvent>(HandleMouseDown);
            RegisterCallback<FocusOutEvent>(HandleFocusOut);

            Add(Root);
            Root.Add(Label);
            Root.Add(Text);
            Root.Add(Type);
        }

        public void SetLabel(string text)
        {
            Label.text = string.IsNullOrEmpty(text) ? emptyText : text;
        }

        protected virtual void HandleFocusOut(FocusOutEvent evt)
        {
            Label.style.display = DisplayStyle.Flex;
            SetLabel(Text.value);
            Text.style.display = DisplayStyle.None;
        }

        private void HandleMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                Label.style.display = DisplayStyle.None;
                Text.style.display = DisplayStyle.Flex;
                Text.Focus();
            }
        }

        public override void BindProperty(SerializedProperty property)
        {
            var assetID = property.FindPropertyRelative("Id").FindPropertyRelative("Value")
                .longValue;
            var dataAsset = (HNSFState)QuantumUnityDB.GetGlobalAssetEditorInstance(new AssetGuid(assetID));

            var so = new SerializedObject(dataAsset);
            var child = so.FindProperty("Label"); //.FirstString();
            if (child != null)
            {
                SetLabel(child.stringValue);
                Text.BindProperty(child);
            }
            else
            {
                Debug.Log("Not Found.");
            }
        }

        public override void Unbind()
        {
            SetLabel("");
            Text.Unbind();
        }
    }
}