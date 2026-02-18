using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using HnSF.core.state;
using Quantum;
using Quantum.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = Quantum.Button;

namespace HnSF
{
    public class FighterEditorWindow : EditorWindow
    {
        [SerializeField] private HnSFConfigurationAsset configAsset = null;
        [SerializeField] private IFighterDefinition fighterDefinition = null;
        [SerializeField] private int currentSelectedModIndex;

        [SerializeField] private List<string> allStatesetOptions = new List<string>();
        [SerializeField] private List<string> allStatesetGuids = new List<string>();
        [SerializeField] private List<HNSFStateSet> allStatesets = new List<HNSFStateSet>();

        [SerializeField] private string stateEditTabCreateStateName = "New State";
        //[SerializeField] private string stateEditTabCreateStateSaveLocation = "New State";
        
        // StateSets : StateGroups : States
        List<List<List<(HNSFState, HNSFStateSet)>>> setToGroupToStateList = new List<List<List<(HNSFState, HNSFStateSet)>>>(); 
        
        [OnOpenAsset]
        public static bool OpenGraphAsset(int instanceID, int line)
        {
            var asset = EditorUtility.EntityIdToObject(instanceID);
            if (asset is not IFighterDefinition definition) return false;

            var ew = OpenWindow(definition);
            ew.Focus();
            return true;
        }
        
        public static FighterEditorWindow OpenWindow(IFighterDefinition fighterDefinition)
        {
            var wnd = CreateWindow<FighterEditorWindow>();
            wnd.fighterDefinition = fighterDefinition;
            wnd.titleContent = new GUIContent(string.IsNullOrEmpty(fighterDefinition.Name) ? "Fighter Editor" : $"{fighterDefinition.Name} Editor");
            wnd.minSize = new Vector2(300, 300);
            return wnd;
        }
        
        private void FindConfigurationAsset()
        {
            var cAssets = AssetDatabase.FindAssets($"t:{nameof(HnSFConfigurationAsset)}");
            if (cAssets.Length > 0) configAsset = AssetDatabase.LoadAssetAtPath<HnSFConfigurationAsset>(AssetDatabase.GUIDToAssetPath(cAssets[0]));
        }
        
        private void UpdateStatesetInfo()
        {
            if (fighterDefinition == null) return;
            
            allStatesetOptions.Clear();
            allStatesetGuids.Clear();
            allStatesets.Clear();

            var battleActorDefinition = GetBattleActorDefinition();
            if (battleActorDefinition == null)
            {
                Debug.LogError("No battle actor definition.");
                return;
            }
            
            for (int i = 0; i < battleActorDefinition.statesets.Count; i++)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance<HNSFStateSet>(battleActorDefinition.statesets[i], out var stateSet))
                {
                    allStatesetOptions.Add("?");
                    allStatesetGuids.Add(null);
                    allStatesets.Add(null);
                    continue;
                }

                var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(stateSet));
                allStatesetOptions.Add(string.IsNullOrEmpty(stateSet.Label) ? assetGuid : stateSet.Label);
                allStatesetGuids.Add(assetGuid);
                allStatesets.Add(stateSet);
            }
        }

        public void CreateGUI()
        {
            //var so = new SerializedObject(this);
            rootVisualElement.Clear();
            
            FindConfigurationAsset();

            if (configAsset == null)
            {
                return;
            }

            UpdateStatesetInfo();
            
            var topTabView = new TabView();
            topTabView.style.flexGrow = 1;
            topTabView.Q<VisualElement>(name: "unity-tab-view__header-container").style.marginBottom = 8;
            rootVisualElement.Add(topTabView);
            
            var tabFighterUnityDefinition = new Tab("General Definition");
            tabFighterUnityDefinition.name = "TabGeneralDefinition";
            topTabView.Add(tabFighterUnityDefinition);
            CreateTabUI_TabGeneralDefinition(tabFighterUnityDefinition);
            
            var tabQuantumDefinition = new Tab("Quantum Definition");
            tabQuantumDefinition.name = "TabQuantumDefinition";
            topTabView.Add(tabQuantumDefinition);
            CreateTabUI_QuantumDefinition(tabQuantumDefinition);
            
            CreateTab_StateEditing();
        }

        private void CreateTab_StateEditing()
        {
            var topTabView = rootVisualElement.Q<TabView>();
            
            var tabStateEditing = new Tab("State Editing");
            tabStateEditing.name = "TabStateEditing";
            topTabView.Add(tabStateEditing);
            CreateTabUI_StateEditing(tabStateEditing);
        }

        private void CreateTabUI_StateEditing()
        {
            CreateTabUI_StateEditing(rootVisualElement.Q<Tab>("TabStateEditing"));
        }

        private void CreateTabUI_StateEditing(Tab tabStateEditing)
        {
            tabStateEditing.Clear();
            if (fighterDefinition == null) return;
            var qfd = GetBattleActorDefinition();
            if(qfd == null) return;

            var selfSo = new SerializedObject(this);

            var refreshButton = new UnityEngine.UIElements.Button();
            refreshButton.text = "Refresh All";
            refreshButton.clicked += CreateTabUI_StateEditing;
            tabStateEditing.Add(refreshButton);
            
            // State General
            var generalVisualTree = Resources.Load<VisualTreeAsset>("UXML/HnSF_FighterEditor_StateEdit_General");
            generalVisualTree.CloneTree(tabStateEditing.contentContainer);

            tabStateEditing.Q<UnityEngine.UIElements.Button>("CreateNewStateToggle").clicked += WhenCreateNewStateToggled;
            var cnse = tabStateEditing.Q<VisualElement>("CreateNewStateElement");
            cnse.style.display = DisplayStyle.None;
            
            var newStateElementStatesetDropdown = cnse.Q<DropdownField>("StatesetDropdown");
            newStateElementStatesetDropdown.choices = allStatesetOptions;
            if (allStatesetOptions.Count > 0) newStateElementStatesetDropdown.index = 0;

            newStateElementStatesetDropdown.RegisterValueChangedCallback(StateEditingTab_WhenNewStateStatesetDropdownChanged);
            
            var newStateElementStateNameField = cnse.Q<TextField>("StateName");
            newStateElementStateNameField.BindProperty(selfSo.FindProperty(nameof(stateEditTabCreateStateName)));

            var newStateElementCreateButton = cnse.Q<UnityEngine.UIElements.Button>("Create");
            newStateElementCreateButton.clicked += StateTab_WhenCreateNewStateClicked;
            
            // State Listing
            var toolbar = new Toolbar();
            tabStateEditing.Add(toolbar);

            var searchField = new ToolbarPopupSearchField();
            searchField.style.flexGrow = 1;
            toolbar.Add(searchField);
            searchField.RegisterValueChangedCallback(WhenStateSearchFieldValueChanged);

            var mainScrollView = new ScrollView();
            mainScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            mainScrollView.mode = ScrollViewMode.Vertical;
            mainScrollView.style.flexGrow = 1;

            BuildStateLists();
            
            for (int i = 0; i < qfd.statesets.Count; i++)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(qfd.statesets[i], out HNSFStateSet stateSet)) continue;

                var stateSetSo = new SerializedObject(stateSet);
                
                var stateSetLabel = new RenamableLabel();
                stateSetLabel.style.fontSize = 28;
                stateSetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                var stateSetLabelSp = stateSetSo.FindProperty(nameof(HNSFStateSet.Label));
                stateSetLabel.BindProperty(stateSetLabelSp);
                mainScrollView.Add(stateSetLabel);
                
                var templatePropertyField = new PropertyField();
                templatePropertyField.BindProperty(stateSetSo.FindProperty(nameof(HNSFStateSet.template)));
                templatePropertyField.RegisterValueChangeCallback(StateEditingTab_WhenStateSetTemplateChanged);
                templatePropertyField.style.marginLeft = 0;
                mainScrollView.Add(templatePropertyField);
                
                for(int w = 0; w < setToGroupToStateList[i].Count; w++)
                {
                    int savedI = i;
                    int savedW = w;
                    //var sg = stateSet.stateGroups[w];
                    bool handlingInheritedStates = w == setToGroupToStateList[i].Count - 1;
                    
                    if (w < setToGroupToStateList[i].Count - 1)
                    {
                        var stateGroupLabel = new RenamableLabel();
                        stateGroupLabel.style.fontSize = 18;
                        var labelSp = stateSetSo.FindProperty(nameof(stateSet.stateGroups)).GetArrayElementAtIndex(w)
                            .FindPropertyRelative(nameof(HNSFStateSet.StateGrouping.label));
                        stateGroupLabel.BindProperty(labelSp);
                        mainScrollView.Add(stateGroupLabel);
                    }
                    else
                    {
                        var stateGroupLabel = new Label();
                        stateGroupLabel.style.fontSize = 18;
                        stateGroupLabel.text = "Inherited States";
                        mainScrollView.Add(stateGroupLabel);
                    }

                    Func<VisualElement> makeItem = () => new FighterEditorStateListViewItem();
                    Func<VisualElement> makeNoneItem = () => new VisualElement();
                    //Action<VisualElement, int> bindItem = (e, i) => ((FighterEditorStateListViewItem)e).Bind(stateSet, QuantumUnityDB.GetGlobalAssetEditorInstance(sg.states[i]));
                    Action<VisualElement, int> bindItem = (e, i) =>
                    {
                        
                        ((FighterEditorStateListViewItem)e).Bind(stateSet, setToGroupToStateList[savedI][savedW][i].Item2,
                            setToGroupToStateList[savedI][savedW][i].Item1, !handlingInheritedStates);
                        ((FighterEditorStateListViewItem)e).onStateCreated.RemoveAllListeners();
                        ((FighterEditorStateListViewItem)e).onStateDeleted.RemoveAllListeners();
                        ((FighterEditorStateListViewItem)e).onStateCreated.AddListener(RefreshStateListAndReturnScrollView);
                        ((FighterEditorStateListViewItem)e).onStateDeleted.AddListener(RefreshStateListAndReturnScrollView);
                        
                    };
                    
                    var stateGroupStateListView = new ListView();
                    stateGroupStateListView.name = $"StateGroupListView{w}";
                    stateGroupStateListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                    stateGroupStateListView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
                    stateGroupStateListView.reorderable = false;
                    stateGroupStateListView.makeNoneElement = makeNoneItem;
                    stateGroupStateListView.makeItem = makeItem;
                    stateGroupStateListView.bindItem = bindItem;
                    stateGroupStateListView.itemsSource = setToGroupToStateList[i][w];
                    stateGroupStateListView.selectionType = SelectionType.None;
                    stateGroupStateListView.style.minHeight = 200;
                    stateGroupStateListView.style.marginTop = 10;
                    mainScrollView.Add(stateGroupStateListView);
                    
                    /*
                    stateGroupStateListView.itemsChosen += (selectedItems) =>
                    {
                        foreach (var item in selectedItems)
                        {
                            
                        }
                    };*/
                }
            }
            
            tabStateEditing.Add(mainScrollView);
        }

        private void RefreshStateListAndReturnScrollView()
        {
            CreateTabUI_StateEditing();
        }

        private void StateEditingTab_WhenNewStateStatesetDropdownChanged(ChangeEvent<string> evt)
        {
            StateEditingTab_UpdateNewStateStateGroups();
        }

        private void StateEditingTab_UpdateNewStateStateGroups()
        {
            var tabStateEditing = rootVisualElement.Q<Tab>("TabStateEditing");
            var cnse = tabStateEditing.Q<VisualElement>("CreateNewStateElement");
            var newStateElementStatesetDropdown = cnse.Q<DropdownField>("StatesetDropdown");
            var sgd = cnse.Q<DropdownField>("StateGroupDropdown");

            var idx = allStatesetOptions.IndexOf(newStateElementStatesetDropdown.value);
            if (idx == -1) return;
            //Debug.Log($"idx {idx} : {newStateElementStatesetDropdown.value}");

            var stateset = AssetDatabase.LoadAssetByGUID<HNSFStateSet>(new GUID(allStatesetGuids[idx]));
            if (stateset == null) return;
            
            var groupList = new List<string>();
            for(int i = 0; i < stateset.stateGroups.Count; i++) groupList.Add(string.IsNullOrEmpty(stateset.stateGroups[i].label) ? $"Grouping {i+1}" : stateset.stateGroups[i].label);
            groupList.Add("New Group");
            sgd.choices = groupList;
        }

        private void StateEditingTab_WhenStateSetTemplateChanged(SerializedPropertyChangeEvent evt)
        {
            
        }

        private void StateTab_WhenCreateNewStateClicked()
        {
            if (string.IsNullOrEmpty(stateEditTabCreateStateName)) return;
            
            var tabStateEditing = rootVisualElement.Q<Tab>("TabStateEditing");
            var cnse = tabStateEditing.Q<VisualElement>("CreateNewStateElement");
            var newStateElementStatesetDropdown = cnse.Q<DropdownField>("StatesetDropdown");
            if (newStateElementStatesetDropdown.index == -1) return;
            var newStateElementStatesetGroupDropdown = cnse.Q<DropdownField>("StateGroupDropdown");
            if (newStateElementStatesetGroupDropdown.choices.Count == 0) return;
            
            var statesetGuid = allStatesetGuids[newStateElementStatesetDropdown.index];
            var stateSet = AssetDatabase.LoadAssetAtPath<HNSFStateSet>(AssetDatabase.GUIDToAssetPath(statesetGuid));
            if (stateSet == null) return;

            var saveFolder = AssetDatabase.GUIDToAssetPath(statesetGuid);
            
            var saveLocation = EditorUtility.SaveFilePanelInProject("Save State Asset", $"{stateEditTabCreateStateName.Replace(" ", "")}", 
                "asset", "Please give the location to save the state.", Path.GetDirectoryName(saveFolder));
            if (string.IsNullOrEmpty(saveLocation)) return;
            
            var newState = ScriptableObject.CreateInstance<HNSFState>();
            newState.Label = stateEditTabCreateStateName;

            var assPath = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
            
            AssetDatabase.CreateAsset(newState, assPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            
            int groupIndex = 0;
            if (newStateElementStatesetGroupDropdown.index == newStateElementStatesetGroupDropdown.choices.Count - 1)
            {
                stateSet.stateGroups.Add(new HNSFStateSet.StateGrouping()
                {
                    label = "New Group",
                    states = new List<AssetRef<HNSFState>>()
                });
                groupIndex = stateSet.stateGroups.Count - 1;
            }
            else
            {
                for (int i = 0; i < stateSet.stateGroups.Count; i++)
                {
                    if (stateSet.stateGroups[i].label !=
                        newStateElementStatesetGroupDropdown.choices[newStateElementStatesetGroupDropdown.index])
                        continue;
                    groupIndex = i;
                    break;
                }
            }

            var realStateAsset = AssetDatabase.LoadMainAssetAtPath(assPath) as HNSFState;
            
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(realStateAsset, out string gotGuid, out long gotLocalId)
                && GUID.TryParse(gotGuid, out GUID resultGuid))
            {
                realStateAsset.Guid = QuantumUnityDBUtilities.CreateDeterministicAssetGuid(resultGuid, gotLocalId);
            }
            EditorUtility.SetDirty(realStateAsset);
            QuantumUnityDB.Global.AddAsset(realStateAsset);
            
            stateSet.stateGroups[groupIndex].states.Add(realStateAsset);
            
            EditorUtility.SetDirty(stateSet);
            Selection.activeObject = realStateAsset;
            
            _ = RebuildListsAfterTime();
        }

        private async UniTask RebuildListsAfterTime()
        {
            await UniTask.WaitForSeconds(0.2f);
            
            BuildStateLists();
            
            var allListViews = rootVisualElement.Query<ListView>().Build();
            foreach (var lview in allListViews)
            {
                lview.Rebuild();
            }
        }
        
        private void WhenCreateNewStateToggled()
        {
            var tabStateEditing = rootVisualElement.Q<Tab>("TabStateEditing");

            var cnse = tabStateEditing.Q<VisualElement>("CreateNewStateElement");
            cnse.style.display = cnse.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;

            if(cnse.style.display == DisplayStyle.Flex) StateEditingTab_UpdateNewStateStateGroups();
        }

        private void BuildStateLists()
        {
            if (fighterDefinition == null) return;
            var qfd = GetBattleActorDefinition();
            if(qfd == null) return;
            
            while(setToGroupToStateList.Count < qfd.statesets.Count) setToGroupToStateList.Add(new List<List<(HNSFState, HNSFStateSet)>>());
            while (setToGroupToStateList.Count > qfd.statesets.Count) setToGroupToStateList.RemoveAt(setToGroupToStateList.Count-1);
            
            for (int i = 0; i < qfd.statesets.Count; i++)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(qfd.statesets[i], out HNSFStateSet stateSet)) continue;
                var stateSetSo = new SerializedObject(stateSet);

                while (setToGroupToStateList[i].Count < (stateSet.stateGroups.Count+1)) setToGroupToStateList[i].Add(new List<(HNSFState, HNSFStateSet)>());
                while (setToGroupToStateList[i].Count > (stateSet.stateGroups.Count+1)) setToGroupToStateList[i].RemoveAt(setToGroupToStateList[i].Count-1);
                
                for (int w = 0; w < stateSet.stateGroups.Count; w++)
                {
                    setToGroupToStateList[i][w].Clear();
                    for (int a = 0; a < stateSet.stateGroups[w].states.Count; a++)
                    {
                        var sa = QuantumUnityDB.GetGlobalAssetEditorInstance(stateSet.stateGroups[w].states[a]);
                        setToGroupToStateList[i][w].Add((sa, stateSet));
                    }
                }

                setToGroupToStateList[i][^1].Clear();
                if(stateSet.template.IsValid && QuantumUnityDB.TryGetGlobalAssetEditorInstance(stateSet.template, out HNSFStateSet templateStateSet)) RecursivelyAddStatesFromTemplate(templateStateSet, qfd, i);
            }
        }

        private void RecursivelyAddStatesFromTemplate(HNSFStateSet templateStateSet, BattleActorDefinition qfd, int stateSetIndex)
        {
            for (int w = 0; w < templateStateSet.stateGroups.Count; w++)
            {
                for (int a = 0; a < templateStateSet.stateGroups[w].states.Count; a++)
                {
                    var sa = templateStateSet.stateGroups[w].states[a];

                    if (QuantumUnityDB.TryGetGlobalAssetEditorInstance(sa, out var stateAsset))
                    {
                        setToGroupToStateList[stateSetIndex][^1].Add((stateAsset, templateStateSet));
                    }
                }
            }
            
            if(templateStateSet.template.IsValid && QuantumUnityDB.TryGetGlobalAssetEditorInstance(templateStateSet.template, out HNSFStateSet templateTemplateStateSet))
                RecursivelyAddStatesFromTemplate(templateTemplateStateSet, qfd, stateSetIndex);
        }

        private void WhenStateSearchFieldValueChanged(ChangeEvent<string> evt)
        {
            BuildStateLists();
            if (string.IsNullOrEmpty(evt.newValue)) return;

            /*
            for (int i = 0; i < setToGroupToStateList.Count; i++)
            {
                for (int w = 0; w < setToGroupToStateList[i].Count; w++)
                {
                    for (int a = setToGroupToStateList[i][w].Count - 1; a >= 0; a--)
                    {
                        var state = setToGroupToStateList[i][w][a];
                        if (state == null) continue;
                        var sLabel = state.Label.ToLower().Replace(" ", "");
                        if (sLabel.Contains(evt.newValue)) continue;
                        setToGroupToStateList[i][w].RemoveAt(a);
                    }
                }
            }*/
        }

        private void CreateTabUI_QuantumDefinition(Tab tabQuantumDefinition)
        {
            if (fighterDefinition == null) return;
            var qfd = GetBattleActorDefinition();
            if(qfd == null) return;
            
            Editor editor = Editor.CreateEditor(qfd);
            IMGUIContainer inspectorIMGUI = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
            tabQuantumDefinition.Add(inspectorIMGUI);
        }

        private void CreateTabUI_TabGeneralDefinition(Tab tabFightersList)
        {
            if (fighterDefinition == null) return;
            Editor editor = Editor.CreateEditor(fighterDefinition);
            IMGUIContainer inspectorIMGUI = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
            tabFightersList.Add(inspectorIMGUI);
        }

        private BattleActorDefinition GetBattleActorDefinition()
        {
            if (fighterDefinition is AddressablesFighterDefinition afd)
            { 
                return afd.quantumDefinition.editorAsset;
            }
#if HNSF_UMOD
            else if (fighterDefinition is UModFighterDefinition umafd)
            {
                // TODO
            }
#endif
            return null;
        }
    }
}
