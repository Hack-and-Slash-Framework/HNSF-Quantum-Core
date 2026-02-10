using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HnSF {
    public class FightersManagerModListViewItem : VisualElement {
        
        [SerializeField] private BaseModInfoAsset modInfoAsset;
        [SerializeField] private List<IFighterDefinition> allFighters = new List<IFighterDefinition>();
        [SerializeField] public Action<IFighterDefinition> onSelectFighter = null;
        
        public FightersManagerModListViewItem()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/HnSF_FightersManager_FighterView_ModListItem");
            visualTree.CloneTree(this);
        }
        
        private void BuildFightersList()
        {
            var fAssets = AssetDatabase.FindAssets($"t:{nameof(IFighterDefinition)}", new string[] { Path.GetDirectoryName(AssetDatabase.GetAssetPath(modInfoAsset)) });
            allFighters.Clear();

            for (int i = 0; i < fAssets.Length; i++)
            {
                var ast = AssetDatabase.LoadAssetAtPath<IFighterDefinition>(AssetDatabase.GUIDToAssetPath(fAssets[i]));
                if (ast == null) continue;
                allFighters.Add(ast);
            }
        }

        public void Bind(BaseModInfoAsset modInfo)
        {
            this.Q<Label>("ModName").text = modInfo.ModName.ToUpper();
            
            this.modInfoAsset = modInfo;
            BuildFightersList();

            Func<VisualElement> makeItem = () => new Label();
            Func<VisualElement> makeNoneItem = () => new VisualElement();
            Action<VisualElement, int> bindItem = (e, i) => ((Label)e).text = allFighters[i].Name;
            
            var fighterListView = this.Q<ListView>();
            fighterListView.reorderable = false;
            fighterListView.makeNoneElement = makeNoneItem;
            fighterListView.makeItem = makeItem;
            fighterListView.bindItem = bindItem;
            fighterListView.itemsSource = allFighters;
            fighterListView.selectionType = SelectionType.Single;
            
            fighterListView.itemsChosen += (selectedItems) =>
            {
                foreach (var item in selectedItems)
                {
                    onSelectFighter?.Invoke(item as IFighterDefinition);
                }

            };
        }
    }
}