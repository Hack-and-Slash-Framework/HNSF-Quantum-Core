using System.Collections.Generic;
using System.IO;
using System.Linq;
using HnSF.core.state;
using Quantum;
using Quantum.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace HnSF
{
    public class FighterEditorStateListViewItem : VisualElement
    {
        [SerializeField] private BaseModInfoAsset modInfoAsset;
        [SerializeField] private HNSFStateSet workingStateSet;
        [SerializeField] private HNSFStateSet stateSet;
        [SerializeField] private HNSFState state;
        [SerializeField] private bool allowModifications = true;

        public UnityEvent onStateCreated = new UnityEvent();
        public UnityEvent onStateDeleted = new UnityEvent();
        
        public FighterEditorStateListViewItem()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/HnSF_FighterEditor_StateListItem");
            visualTree.CloneTree(this);
            
            var menuManipulator = new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Select State Asset", (x) => { EditorGUIUtility.PingObject(state); });
                if (allowModifications )
                {
                    evt.menu.AppendAction("Delete", (x) =>
                    {
                        if (EditorUtility.DisplayDialog($"Delete state {state.Label}?",
                                "Are you sure you want to delete this state?"
                                , "Yes", "No") == false) return;

                        for (int i = 0; i < stateSet.stateGroups.Count; i++)
                        {
                            stateSet.stateGroups[i].states.Remove(state);
                        }

                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(state));
                        onStateDeleted.Invoke();
                    });

                    foreach (var grouping in workingStateSet.stateGroups)
                    {
                        evt.menu.AppendAction(
                            $"Move To Group/{(string.IsNullOrEmpty(grouping.label) ? "<Unlabeled>" : grouping.label)}",
                            (x) =>
                            {
                                var groupCached = grouping;

                                foreach (var g in workingStateSet.stateGroups)
                                {
                                    g.states.Remove(state);
                                }
                                
                                groupCached.states.Add(state);
                                workingStateSet.RefreshStateList();
                                onStateCreated.Invoke();
                            });
                    }
                }

                if (workingStateSet != stateSet)
                {
                    for (int i = 0; i < workingStateSet.movesets.Length+1; i++)
                    {
                        bool forAllMovesets = i == workingStateSet.movesets.Length;
                        string movesetName = "For All Movesets";
                        bool canLocalize = true;

                        if (forAllMovesets == false)
                        {
                            bool foundMovesetTag =
                                QuantumUnityDB.TryGetGlobalAssetEditorInstance(workingStateSet.movesets[i],
                                    out var movesetTag);
                            movesetName = "In Moveset " + (foundMovesetTag ? movesetTag.label : "Default");

                            if (StateEditorHelpers.StateWithTagExistsForMoveset(state.sharedStateTag,
                                    workingStateSet.movesets[i], workingStateSet)) canLocalize = false;
                        }
                        else
                        {
                            if (StateEditorHelpers.StateWithTagInAllMovesets(state.sharedStateTag, workingStateSet))
                                canLocalize = false;
                        }

                        for (int w = 0; w < workingStateSet.stateGroups.Count + 1; w++)
                        {
                            var movesetIndex = i;
                            var wCached = w;
                            var stateGroupName = w == workingStateSet.stateGroups.Count
                                ? "New Group"
                                : workingStateSet.stateGroups[w].label;
                            
                            if (state.sharedStateTag.IsValid && canLocalize)
                            {
                                evt.menu.AppendAction($"Localize/{movesetName}/{stateGroupName}/Empty",
                                    (x) =>
                                    {
                                        var saveLocation = GetStateSaveLocation();
                                        if (string.IsNullOrEmpty(saveLocation)) return;

                                        var newState = CreateLocalizedState(forAllMovesets, movesetIndex, saveLocation,
                                            wCached, stateGroupName);
                                        state.CopyTo(newState);
                                        EditorUtility.SetDirty(newState);

                                        onStateCreated.Invoke();
                                    });

                                evt.menu.AppendAction($"Localize/{movesetName}/{stateGroupName}/With Base State",
                                    (x) =>
                                    {
                                        var saveLocation = GetStateSaveLocation();
                                        if (string.IsNullOrEmpty(saveLocation)) return;

                                        var newState = CreateLocalizedState(forAllMovesets, movesetIndex, saveLocation,
                                            wCached, stateGroupName);
                                        state.CopyTo(newState);
                                        newState.tags = state.tags.ToArray();
                                        newState.useBaseState = true;
                                        newState.baseState = state;
                                        newState.totalFrames = state.totalFrames;
                                        newState.stateType = state.stateType;
                                        newState.initialGroundedState = state.initialGroundedState;
                                        EditorUtility.SetDirty(newState);

                                        onStateCreated.Invoke();
                                    });

                                evt.menu.AppendAction($"Localize/{movesetName}/{stateGroupName}/Cloned",
                                    (x) =>
                                    {
                                        var saveLocation = GetStateSaveLocation();
                                        if (string.IsNullOrEmpty(saveLocation)) return;

                                        var newState = CreateLocalizedState(forAllMovesets, movesetIndex, saveLocation,
                                            wCached, stateGroupName);
                                        state.CopyTo(newState);
                                        state.CopyDataTo(newState);
                                        EditorUtility.SetDirty(newState);

                                        onStateCreated.Invoke();
                                    });
                            }
                            else
                            {
                                evt.menu.AppendAction($"Clone/{movesetName}/{stateGroupName}/As Is",
                                    (x) =>
                                    {
                                        var saveLocation = GetStateSaveLocation();
                                        if (string.IsNullOrEmpty(saveLocation)) return;
                                    
                                        var newState = CreateLocalizedState(forAllMovesets, movesetIndex, saveLocation, wCached, stateGroupName);
                                        state.CopyTo(newState);
                                        state.CopyDataTo(newState);
                                        EditorUtility.SetDirty(newState);
                                    
                                        onStateCreated.Invoke();
                                    });
                            
                                evt.menu.AppendAction($"Clone/{movesetName}/{stateGroupName}/As Base State",
                                    (x) =>
                                    {
                                        var saveLocation = GetStateSaveLocation();
                                        if (string.IsNullOrEmpty(saveLocation)) return;
                                    
                                        var newState = CreateLocalizedState(forAllMovesets, movesetIndex, saveLocation, wCached, stateGroupName);
                                        state.CopyTo(newState);
                                        newState.tags = state.tags.ToArray();
                                        newState.useBaseState = true;
                                        newState.baseState = state;
                                        newState.totalFrames = state.totalFrames;
                                        newState.stateType = state.stateType;
                                        newState.initialGroundedState = state.initialGroundedState;
                                        EditorUtility.SetDirty(newState);
                                    
                                        onStateCreated.Invoke();
                                    });
                            }
                        }
                    }
                }
            });
            this.AddManipulator(menuManipulator);

            var clickable = new Clickable(() =>
            {
                var window = StateSetEditorWindow.GetOrOpenWindow(stateSet);
                window.SetSelection(state);
            });
            clickable.activators.Clear();
            clickable.activators.Add(new ManipulatorActivationFilter
                { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });

            this.AddManipulator(clickable);
        }

        private HNSFState CreateLocalizedState(bool forAllMovesets, int movesetIndex, string saveLocation, int wCached, string stateGroupName)
        {
            HNSFState newState = ScriptableObject.CreateInstance<HNSFState>();
            
            if (forAllMovesets)
            {
                newState.applyToAllMovesets = true;
            }
            else
            {
                newState.applyToAllMovesets = false;
                newState.movesetTags = new[] { workingStateSet.movesets[movesetIndex] };
            }
            
            var assPath = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
            AssetDatabase.CreateAsset(newState, assPath);
            AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assPath));
            AssetDatabase.ImportAsset(assPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
                                    
            int groupIndex = 0;
            if (wCached == workingStateSet.stateGroups.Count)
            {
                workingStateSet.stateGroups.Add(new HNSFStateSet.StateGrouping()
                {
                    label = "New Group",
                    states = new List<AssetRef<HNSFState>>()
                });
                groupIndex = workingStateSet.stateGroups.Count - 1;
            }
            else
            {
                for (int i = 0; i < workingStateSet.stateGroups.Count; i++)
                {
                    if (workingStateSet.stateGroups[i].label != stateGroupName)
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
            
            workingStateSet.stateGroups[groupIndex].states.Add(realStateAsset);
            workingStateSet.RefreshStateList();
            
            EditorUtility.SetDirty(workingStateSet);
            Selection.activeObject = realStateAsset;
            return realStateAsset;
        }

        private string GetStateSaveLocation()
        {
            var saveFolder = AssetDatabase.GetAssetPath(workingStateSet);
            var saveLocation = EditorUtility.SaveFilePanelInProject("Save State Asset", $"{state.name}", 
                "asset", "Please give the location to save the state.", Path.GetDirectoryName(saveFolder));
            return saveLocation;
        }

        public void Bind(HNSFStateSet workingStateSet, HNSFStateSet stateSet, HNSFState state,
            bool allowModifications = true)
        {
            this.workingStateSet = workingStateSet;
            this.stateSet = stateSet;
            this.state = state;

            if (state == null) return;

            this.allowModifications = allowModifications;

            this.Q<Label>("StateName").BindProperty(new SerializedObject(state).FindProperty(nameof(HNSFState.Label)));

            var inheritanceList = "";
            this.Q<Label>("StateInheritanceList").style.display = DisplayStyle.None;
            if (state.useBaseState && QuantumUnityDB.TryGetGlobalAssetEditorInstance(state.baseState, out var bs))
            {
                BuildInheritanceList(bs, ref inheritanceList);
                this.Q<Label>("StateInheritanceList").text = "Inheritance: " + inheritanceList;
                this.Q<Label>("StateInheritanceList").style.display = DisplayStyle.Flex;
            }

            Editor editor = Editor.CreateEditor(this.state);
            var defaultInspectorContainer = this.Q<Foldout>("DefaultInspectorFoldout");
            IMGUIContainer inspectorIMGUI = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
            defaultInspectorContainer.Add(inspectorIMGUI);
        }

        private string BuildInheritanceList(HNSFState evaluatingState, ref string currentString)
        {
            if (evaluatingState == null) return "";

            currentString += string.IsNullOrEmpty(currentString)
                ? $"{evaluatingState.Label}"
                : $" < {evaluatingState.Label}";

            if (evaluatingState.useBaseState &&
                QuantumUnityDB.TryGetGlobalAssetEditorInstance(evaluatingState.baseState, out var baseState))
            {
                BuildInheritanceList(baseState, ref currentString);
            }

            return currentString;
        }
    }
}