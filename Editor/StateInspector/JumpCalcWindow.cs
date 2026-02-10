using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class JumpCalcWindow : EditorWindow
{
    private FloatField heightField;
    private FloatField timeField;
    private TextField jumpForce;
    private TextField gravity;
    
    [MenuItem("Tools/Jump Calculator")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<JumpCalcWindow>();
        wnd.titleContent = new GUIContent("Jump Calculator");
    }
    
    void CreateGUI()
    {
        var root = rootVisualElement;

        heightField = new FloatField();
        heightField.label = "Jump Height";
        heightField.tooltip = "The peak height that the jump will reach.";
        heightField.RegisterValueChangedCallback(WhenValueChanged);
        root.Add(heightField);
        
        timeField = new FloatField();
        timeField.label = "Jump Duration";
        timeField.tooltip = "The total length of the jump, from start-apex-start.";
        timeField.RegisterValueChangedCallback(WhenValueChanged);
        root.Add(timeField);

        jumpForce = new TextField();
        jumpForce.value = "Jump Force: ?";
        jumpForce.tooltip = "The upwards force necessary for the wanted jump.";
        root.Add(jumpForce);

        gravity = new TextField();
        gravity.value = "Gravity: ?";
        gravity.tooltip = "The continuous gravity force necessary for the wanted jump.";
        root.Add(gravity);
    }

    private void WhenValueChanged(ChangeEvent<float> evt)
    {
        if (heightField.value == 0 || timeField.value == 0)
        {
            jumpForce.value = "Jump Force: ?";
            gravity.value = "Gravity: ?";
            return;
        }

        var apexTime = timeField.value / 2.0f;
        var wantedJumpForce = (2.0f * heightField.value) / apexTime;
        var wantedGravity = (2.0f * heightField.value) / Mathf.Pow(timeField.value / 2.0f, 2.0f);

        jumpForce.value = $"Jump Force: {wantedJumpForce.ToString()}";
        gravity.value = $"Gravity: {wantedGravity.ToString()}";
    }
}