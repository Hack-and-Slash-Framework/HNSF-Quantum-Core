using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AdvancedTypeModal : EditorWindow
{
    public event Action<Type> OnItemSelected;
    Type[] m_Types;

    private Color borderColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    
    public static AdvancedTypeModal Show(Vector2 pos, IEnumerable<Type> types, int maxLineCount)
    {
        var window = ScriptableObject.CreateInstance<AdvancedTypeModal>();
        window.position = new Rect(pos.x, pos.y, 400, 500);
        window.ShowPopup();
        window.SetTypes(types);
        return window;
    }
    
    void CreateGUI()
    {
        rootVisualElement.style.borderLeftColor = new StyleColor(borderColor);
        rootVisualElement.style.borderLeftWidth = new StyleFloat(1.0f);
        rootVisualElement.style.borderRightColor = new StyleColor(borderColor);
        rootVisualElement.style.borderRightWidth = new StyleFloat(1.0f);
        rootVisualElement.style.borderTopColor = new StyleColor(borderColor);
        rootVisualElement.style.borderTopWidth = new StyleFloat(1.0f);
        rootVisualElement.style.borderBottomColor = new StyleColor(borderColor);
        rootVisualElement.style.borderBottomWidth = new StyleFloat(1.0f);
        AdvancedTypePopup atp = new AdvancedTypePopup();
        atp.OnItemSelected += WhenItemSelected;
        rootVisualElement.Add(atp);
    }

    private void WhenItemSelected(Type obj)
    {
        OnItemSelected?.Invoke(obj);
        Close();
    }

    public void SetTypes (IEnumerable<Type> types) {
        m_Types = types.ToArray();

        var atp = rootVisualElement.Q<AdvancedTypePopup>();
        atp.AddTo(m_Types);
    }

    private void OnLostFocus()
    {
        Close();
    }
}