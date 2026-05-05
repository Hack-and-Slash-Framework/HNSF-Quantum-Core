using System;
using System.Collections.Generic;
using HnSF.core.state;
using Quantum;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;


namespace HnSF
{
    public class StateSetEditorWindow : EditorWindow
    {
        public VisualTreeAsset template;
        //public VisualTreeAsset stateListItemTemplate;
        
        [SerializeField] public HNSFStateSet stateSet;
        [SerializeField] private StateTimelineEditorView stateTimelineEditorView;
        

        // Previewing
        [SerializeField] public bool inPreviewMode;
        [SerializeField] public bool previewChangeInProgress;
        [SerializeField] private StatePreviewEditorWindow previewEditorWindow;
        
        [OnOpenAsset]
        public static bool OpenGraphAsset(int instanceID, int line)
        {
            var asset = EditorUtility.EntityIdToObject(instanceID);
            if (!(asset is HNSFStateSet)) return false;

            var ew = OpenWindow(asset as HNSFStateSet);
            ew.Focus();
            return true;
        }
        
        public static StateSetEditorWindow OpenWindow(HNSFStateSet stateSet)
        {
            StateSetEditorWindow wnd = CreateWindow<StateSetEditorWindow>();
            wnd.titleContent = new GUIContent(String.IsNullOrEmpty(stateSet.name) ? "State Set Editor" : stateSet.name);
            wnd.minSize = new Vector2(900, 500);
            wnd.stateSet = stateSet;
            wnd.CheckStateGroups();
            wnd.RefreshAll(true);
            return wnd;
        }

        public static StateSetEditorWindow GetOrOpenWindow(HNSFStateSet stateSet)
        {
            foreach (var window in Resources.FindObjectsOfTypeAll(typeof(StateSetEditorWindow)))
            {
                var wnd = window as StateSetEditorWindow;
                if(wnd.stateSet != stateSet) continue;
                //wnd.CheckStateGroups();
                return wnd;
            }
            return OpenWindow(stateSet);
        }

        public virtual void CheckStateGroups()
        {
            if(stateSet.stateGroups.Count == 0)
                stateSet.stateGroups.Add(new HNSFStateSet.StateGrouping());

            if (stateSet.states == null) return;
            
            for (int i = 0; i < stateSet.states.Count; i++)
            {
                bool set = false;
                foreach (var sg in stateSet.stateGroups)
                {
                    if (sg.states.Contains(stateSet.states[i]))
                    {
                        set = true;
                        break;
                    }
                }
                if (set) continue;
                
                stateSet.stateGroups[0].states.Add(stateSet.states[i]);
            }
        }

        public virtual void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            template.CloneTree(root);
            
            var panelRight = root.Q(name: "panel-right");
            StateTimelineEditorView stev = new StateTimelineEditorView();
            stev.style.flexGrow = new StyleFloat(1);
            stateTimelineEditorView = stev;
            panelRight.Add(stev);
            
            RefreshAll(true);

            var playControlPreview = root.Q<Button>("preview");
            playControlPreview.clicked += AttemptTogglePreview;
            
            var playControlBeginning = root.Q<Button>("beginning");
            playControlBeginning.text = "";
            playControlBeginning.iconImage = Background.FromTexture2D((EditorGUIUtility.IconContent("d_Animation.FirstKey").image as Texture2D));
            playControlBeginning.clicked += PreviewButton_Beginning;
            
            var playControlPrevKey = root.Q<Button>("back-frame");
            playControlPrevKey.text = "";
            playControlPrevKey.iconImage = Background.FromTexture2D((EditorGUIUtility.IconContent("d_Animation.PrevKey").image as Texture2D));
            
            var playControlPlayPause = root.Q<Button>("play-pause");
            playControlPlayPause.text = "";
            playControlPlayPause.iconImage = Background.FromTexture2D((EditorGUIUtility.IconContent("d_Animation.Play").image as Texture2D));
            playControlPlayPause.clicked += PreviewButton_TogglePlay;
            
            var playControlNextKey = root.Q<Button>("forward-frame");
            playControlNextKey.text = "";
            playControlNextKey.iconImage = Background.FromTexture2D((EditorGUIUtility.IconContent("d_Animation.NextKey").image as Texture2D));
            playControlNextKey.clicked += PreviewButton_AdvanceFrame;
            
            var playControlEnd = root.Q<Button>("end");
            playControlEnd.text = "";
            playControlEnd.iconImage = Background.FromTexture2D((EditorGUIUtility.IconContent("d_Animation.LastKey").image as Texture2D));
        }

        private void PreviewButton_TogglePlay()
        {
            if (!inPreviewMode || previewEditorWindow == null) return;

            previewEditorWindow.ToggleAutoPlay();
        }

        private void PreviewButton_AdvanceFrame()
        {
            if (!inPreviewMode || previewEditorWindow == null) return;

            previewEditorWindow.TickSimulation();
        }

        private void PreviewButton_Beginning()
        {
            if (!inPreviewMode || previewEditorWindow == null) return;

            if (stateTimelineEditorView.stateAsset)
            {
                previewEditorWindow.SetStateForPlayback(stateTimelineEditorView.stateAsset);
            }
        }

        private void AttemptTogglePreview()
        {
            if (previewChangeInProgress) return;
            Debug.Log("Toggling preview");
            previewChangeInProgress = true;
            if (inPreviewMode)
            {
                previewEditorWindow?.OnWindowClosed.RemoveListener(EndPreviewing);
                previewEditorWindow?.Teardown();
                inPreviewMode = false;
            }
            else
            {
                var previewSettingsAssetGuids = AssetDatabase.FindAssets($"t:{nameof(StatePreviewConfiguration)}");
                if (previewSettingsAssetGuids == null || previewSettingsAssetGuids.Length == 0)
                {
                    Debug.LogError($"Failed setting up state preview: Can not find {nameof(StatePreviewConfiguration)}.");
                    previewChangeInProgress = false;
                    return;
                }
                StatePreviewConfiguration previewSettingsAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(previewSettingsAssetGuids[0]), typeof(StatePreviewConfiguration)) as StatePreviewConfiguration;
                if (previewSettingsAsset == null)
                {
                    Debug.LogError("Failed setting up state preview: Error loading preview configuration asset.");
                    previewChangeInProgress = false;
                    return;
                }

                /*
                if (string.IsNullOrEmpty(previewSettingsAsset.previewScene) ||
                    !SceneManager.GetSceneByName(previewSettingsAsset.previewScene).IsValid())
                {
                    Debug.LogError("Failed setting up state preview: Invalid scene.");
                    previewChangeInProgress = false;
                    return;
                }*/

                if (previewSettingsAsset.simulationSettings == null)
                {
                    Debug.LogError("Failed setting up state preview: Invalid quantum configuration.");
                    previewChangeInProgress = false;
                    return;
                }
                
                previewEditorWindow = EditorWindow.GetWindow<StatePreviewEditorWindow>("State Preview");
                if (previewEditorWindow.Initialize(previewSettingsAsset, stateSet, stateTimelineEditorView.stateAsset))
                {
                    previewEditorWindow.OnWindowClosed.AddListener(EndPreviewing);
                    inPreviewMode = true;
                }
            }

            previewChangeInProgress = false;
        }

        private void EndPreviewing()
        {
            inPreviewMode = false;
        }

        public virtual void RefreshAll(bool refreshData = false)
        {
            if (stateSet == null) return;
            VisualElement root = rootVisualElement;

            var stateScrollView = root.Q(name: "panel-left").Q(name: "container").Q<ScrollView>();
            var stateSearch = root.Q(name: "panel-left").Q<ToolbarSearchField>();
            stateSearch.RegisterValueChangedCallback(HandleSearch);

            var so = new SerializedObject(stateSet);
            
            // CREATE GROUPING FOLDOUTS
            for (int i = 0; i < stateSet.stateGroups.Count; i++)
            {
                Foldout fout = new Foldout();
                fout.name = $"state-foldout-{i}";
                fout.text = stateSet.stateGroups[i].label;
                fout.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                stateScrollView.Add(fout);

                var lview = new SimpleList();
                lview.selectionType = SelectionType.Single;
                lview.name = $"state-lview-{i}";
                lview.RegisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
                lview.SelectionChanged += HandleSelection;
                fout.Add(lview);
                
                lview.BindProperty(so.FindProperty("stateGroups").GetArrayElementAtIndex(i).FindPropertyRelative("states"));
                lview.CreateItem = () =>
                {
                    var lli = new LabelListItem();

                    /*
                    var menuManipulator = new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
                    {
                        evt.menu.AppendAction("Open", (x) =>
                        {
                            PopUpAssetInspector.Create(stateAction);
                        });
                    });
                    lli.AddManipulator(menuManipulator);*/
                    
                    return lli;
                };
            }

            var emptyLabels = root.Query(className: "unity-list-view__empty-label").Build();

            foreach (var emptyLabel in emptyLabels)
            {
                emptyLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        }

        public void SetSelection(HNSFState stateAsset)
        {
            stateTimelineEditorView.SetStateAsset(stateAsset);
        }
        
        private bool _ignoreSelectionEvents;
        
        private void HandleSelection(List<SimpleList.ItemData> sData)
        {
            if (_ignoreSelectionEvents) {
                return;
            }
            
            if (sData.Count == 0) return;
            
            var assetID = sData[0].Property.FindPropertyRelative("Id").FindPropertyRelative("Value")
                .longValue;
            var dataAsset = (HNSFState)QuantumUnityDB.GetGlobalAssetEditorInstance(new AssetGuid(assetID));
            stateTimelineEditorView.SetStateAsset(dataAsset);
        }

        private void HandlePointerDown(PointerDownEvent evt) {
            if (evt.button != 1 && (evt.button == 2 || evt.shiftKey || evt.ctrlKey)) {
                return;
            }

            _ignoreSelectionEvents = true;

            var stateScrollView = rootVisualElement.Q(name: "panel-left").Q(name: "container").Q<ScrollView>();
            for (int i = 0; i < stateSet.stateGroups.Count; i++)
            {
                var slist = stateScrollView.Q<SimpleList>(name: $"state-lview-{i}");
                slist.ClearSelection();
            }

            _ignoreSelectionEvents = false;
        }
        
        private void HandleSearch(ChangeEvent<string> evt) {
            var stateScrollView = rootVisualElement.Q(name: "panel-left").Q(name: "container").Q<ScrollView>();
            
            for (int i = 0; i < stateSet.stateGroups.Count; i++)
            {
                var sfoldout = stateScrollView.Q<Foldout>(name: $"state-foldout-{i}");
                var slist = stateScrollView.Q<SimpleList>(name: $"state-lview-{i}");
                sfoldout.value = true;
                slist.SearchText = evt.newValue;
            }
        }

    }
}