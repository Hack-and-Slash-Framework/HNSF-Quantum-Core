using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AdvancedTypePopup : VisualElement
{
    const int kMaxNamespaceNestCount = 16;
    
    private VisualTreeAsset listElementTemplate;
    
    public event Action<Type> OnItemSelected;

    public List<AdvancedTypePopupItem> folderHistory = new();
    
    public AdvancedTypePopup()
    {
        listElementTemplate = Resources.Load<VisualTreeAsset>("UXML/UXML_AdvancedTypePopup_ListElement");
        
        var visualTree = Resources.Load<VisualTreeAsset>("UXML/UXML_AdvancedTypePopup");
        visualTree.CloneTree(this);
        var searchField = this.Q<ToolbarSearchField>();
        searchField.RegisterValueChangedCallback(WhenSearchChanged);
    }

    private void WhenSearchChanged(ChangeEvent<string> evt)
    {
        var root = this.Q<ScrollView>();

        foreach (var c in root.contentContainer.Children())
        {
            if ((c as AdvancedTypePopupItem).isFolder)
            {
                c.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                continue;
            }
            c.style.display = new StyleEnum<DisplayStyle>(!string.IsNullOrEmpty(evt.newValue)
                                                          && !c.name.Contains(evt.newValue, StringComparison.CurrentCultureIgnoreCase) ? DisplayStyle.None : DisplayStyle.Flex);
        }
    }
    

    public void AddTo(IEnumerable<Type> types)
    {
        var root = this.Q<ScrollView>();
        
        //int itemCount = 0;
        
        var nullItem = new AdvancedTypePopupItem();
        nullItem.Q(name: "folder-label").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        nullItem.Q<Label>().text = "<null>";
        nullItem.OnItemSelected += WhenItemSelected;
        root.Add(nullItem);

        Type[] typeArray = types.ToArray();

        bool isSingleNamespace = true;
        string[] namespaces = new string[kMaxNamespaceNestCount];
        
        foreach (Type type in typeArray)
        {
            string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
            if (splittedTypePath.Length <= 1) continue;

            // If they explicitly want sub category, let them do.
            if (TypeMenuUtility.GetAttribute(type) != null) {
                isSingleNamespace = false;
                break;
            }
            
            for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
            {
                string ns = namespaces[k];
                if (ns == null)
                {
                    namespaces[k] = splittedTypePath[k];
                }else if (ns != splittedTypePath[k])
                {
                    isSingleNamespace = false;
                    break;
                }
            }

            if (!isSingleNamespace) break;
        }
        
        foreach (Type type in typeArray)
        {
            string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
            if (splittedTypePath.Length == 0) {
                continue;
            }

            VisualElement parent = root;
            
            // Add namespace items.
            if (!isSingleNamespace)
            {
                for (int k = 0; k < (splittedTypePath.Length - 1); k++)
                {
                    AdvancedTypePopupItem pItem = root.Q<AdvancedTypePopupItem>(name: splittedTypePath[k]);
                    if (pItem != null)
                    {
                        parent = pItem;
                    }
                    else
                    {
                        var newItem = new AdvancedTypePopupItem(splittedTypePath[k], k <= splittedTypePath.Length-2);
                        newItem.OnItemSelected += WhenFolderSelected;
                        if (parent is AdvancedTypePopupItem)
                        {
                            (parent as AdvancedTypePopupItem).AddChild(newItem);
                        }
                        root.Add(newItem);
                        parent = newItem;
                    }
                }
            }

            var item = new AdvancedTypePopupItem(type.Name, false);
            item.name = type.Name;
            item.t = type;
            item.Q(name: "folder-label").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            item.OnItemSelected += WhenItemSelected;
            
            if(parent is AdvancedTypePopupItem rootItem) rootItem.AddChild(item);
            root.Add(item);
        }
        
        this.Q<ToolbarSearchField>().Focus();
    }
    
    private void WhenFolderSelected(AdvancedTypePopupItem arg1, Type arg2)
    {
        var root = this.Q<ScrollView>();
        
        foreach (var c in root.contentContainer.Children())
        {
                c.style.display = new StyleEnum<DisplayStyle>(arg1.children.Contains(c) ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }

    private void WhenItemSelected(AdvancedTypePopupItem advancedTypePopupItem, Type type)
    {
        OnItemSelected?.Invoke(type);
    }
}
