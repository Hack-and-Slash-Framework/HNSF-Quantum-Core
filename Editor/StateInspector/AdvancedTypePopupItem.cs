using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AdvancedTypePopupItem : VisualElement
{
    public event Action<AdvancedTypePopupItem, Type> OnItemSelected;

    public Type t = null;

    public bool isFolder;

    public List<AdvancedTypePopupItem> children = new();
    
    public AdvancedTypePopupItem()
    {
        var visualTree = Resources.Load<VisualTreeAsset>("UXML/UXML_AdvancedTypePopup_ListElement");
        visualTree.CloneTree(this);
        
        this.RegisterCallback<ClickEvent, VisualElement>(WhenClicked, this);
    }
    
    public AdvancedTypePopupItem(string text, bool isFolder)
    {
        this.name = text;
        
        var visualTree = Resources.Load<VisualTreeAsset>("UXML/UXML_AdvancedTypePopup_ListElement");
        visualTree.CloneTree(this);
        
        this.RegisterCallback<ClickEvent, VisualElement>(WhenClicked, this);

        this.Q<Label>().text = text;

        this.isFolder = isFolder;
    }

    private void WhenClicked(ClickEvent evt, VisualElement userargs)
    {
        OnItemSelected?.Invoke(this, t);
    }

    public void AddChild(AdvancedTypePopupItem item)
    {
        item.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        children.Add(item);
    }
}
