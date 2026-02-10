using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HnSF
{
    public class RenamableLabel : EditableListItem
    {
        protected readonly Label Label;
        protected readonly VisualElement Root;
        public readonly TextField Text;

        public string emptyText = "<empty>";

        public double mouseDownTime;

        public RenamableLabel()
        {
            Root = new VisualElement();
            Root.AddToClassList("editable-item");

            Label = new Label();
            Label.AddToClassList("editable-item__label");
            Text = new TextField { style = { display = DisplayStyle.None } };
            Text.AddToClassList("editable-item__field");

            Label.RegisterCallback<MouseDownEvent>(HandleMouseDown);
            RegisterCallback<FocusOutEvent>(HandleFocusOut);

            Add(Root);
            Root.Add(Label);
            Root.Add(Text);
        }

        public void SetLabel(string text)
        {
            Label.text = string.IsNullOrEmpty(text) ? emptyText : text;
        }

        protected virtual void HandleFocusOut(FocusOutEvent evt)
        {
            if ((EditorApplication.timeSinceStartup - mouseDownTime) < 0.1)
            {
                Text.Focus();
                return;
            }

            Label.style.display = DisplayStyle.Flex;
            SetLabel(Text.value);
            Text.style.display = DisplayStyle.None;
        }

        private void HandleMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                mouseDownTime = EditorApplication.timeSinceStartup;
                Label.style.display = DisplayStyle.None;
                Text.style.display = DisplayStyle.Flex;
                Text.Focus();
            }
        }

        public override void BindProperty(SerializedProperty property)
        {
            var child = property;
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