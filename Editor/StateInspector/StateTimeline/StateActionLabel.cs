using System;
using System.Linq;
using HnSF;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


public class StateActionLabel : VisualElement
{
    [SerializeField] private HNSFState workingState;
    
    [SerializeField] private HNSFState stateAsset;
    [SerializeField] private SerializedObject stateSo;
    [SerializeField] private HNSFStateAction stateActionAsset;
    [SerializeField] private SerializedProperty stateActionSP;

    public delegate void OnChange();
    [SerializeField] public OnChange whenActionChanged;

    public UnityEvent OnIgnoreStatusChanged = new UnityEvent();
    
    public StateActionLabel(HNSFState workingState, HNSFState state, HNSFStateAction action, SerializedObject stateSo, SerializedProperty stateActionSP)
    {
        this.workingState = workingState;
        stateAsset = state;
        this.stateSo = stateSo;
        stateActionAsset = action;
        this.stateActionSP = stateActionSP;
        
        var li = new LabelListItem();
        li.name = $"stateActionLabel_{stateAsset.Guid.ToString()}_{stateActionAsset.id.ToString()}";
        li.emptyText = stateActionAsset.GetType().Name;
        li.SetLabel(stateActionAsset.Label);
        li.style.height = new StyleLength(25);
        li.Text.BindProperty(this.stateActionSP.FindPropertyRelative("Label"));
        this.Add(li);
        
        UpdateUi();
    }

    public void SetupContextMenu(HNSFState state, HNSFStateAction stateAction)
    {
        var v = this.Children().First();
        
        v.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("Edit", (x) =>
            {
                var w = PopUpPropertyInspector.Create(stateAsset, stateSo, stateActionSP);
                //var w = StateTimelineDataEditor.Init(stateTimeline, stateTimeline.data[index].ID);
                //w.onChanged += id => { UpdateData(stateTimeline, stateTimeline.data[index].ID); };
            });
            evt.menu.AppendAction("Add", (x) =>
            {
                var p = GUIUtility.GUIToScreenPoint(StateTimelineEditorView.lastKnownMousePosition);
                var atm = AdvancedTypeModal.Show(p, TypeCache.GetTypesDerivedFrom(typeof(HNSFStateAction)).Where(p =>
                        (p.IsPublic || p.IsNestedPublic) &&
                        !p.IsAbstract &&
                        !p.IsGenericType), 
                    20);
                atm.OnItemSelected += (a) => { WhenItemSelected(stateActionAsset, a); };
            });
            evt.menu.AppendAction("Delete", (x) =>
            {
                DeleteItem(stateActionAsset);
                whenActionChanged?.Invoke();
            });
            evt.menu.AppendAction("Copy", (x) =>
            {
                StateTimelineEditorView.actionCopy = stateActionAsset;
                //CopyStateVariable(stateTimeline, index);
            });
            if (StateTimelineEditorView.actionCopy != null)
            {
                evt.menu.AppendAction("Paste in Place/Without Children", (x) =>
                {
                    PasteInPlaceWithoutChildren(StateTimelineEditorView.actionCopy);
                    whenActionChanged?.Invoke();
                });
                evt.menu.AppendAction("Paste in Place/With Children (Override)", (x) =>
                {
                    PasteInPasteWithChildren(StateTimelineEditorView.actionCopy);
                    whenActionChanged?.Invoke();
                });
                evt.menu.AppendAction("Paste as Child/Without Children", (x) =>
                {
                    PasteAsChildWithoutChildren(StateTimelineEditorView.actionCopy);
                    whenActionChanged?.Invoke();
                });
                evt.menu.AppendAction("Paste as Child/With Children", (x) =>
                {
                    PasteAsChildWithChildren(StateTimelineEditorView.actionCopy);
                    whenActionChanged?.Invoke();
                });
            }
            evt.menu.AppendAction("Move To Top", (x) =>
            {
                MoveActionToTop();
                whenActionChanged?.Invoke();
            });
            evt.menu.AppendAction("Move Up", (x) =>
            {
                MoveActionUp();
                whenActionChanged?.Invoke();
            });
            evt.menu.AppendAction("Move Down", (x) =>
            {
                MoveActionDown();
                whenActionChanged?.Invoke();
            });
            if (workingState != stateAsset)
            {
                if (workingState.ignoredActions.Exists((x) =>
                        x.stateRef == stateAsset && x.actionId == stateActionAsset.id))
                {
                    evt.menu.AppendAction("Unignore", (x) =>
                    {
                        Undo.RecordObject(workingState, $"Ignored action of id {stateActionAsset.id} from {stateAsset.Label}.");
                        
                        for (int i = workingState.ignoredActions.Count - 1; i >= 0; i--)
                        {
                            if(workingState.ignoredActions[i].stateRef == stateAsset && workingState.ignoredActions[i].actionId == stateActionAsset.id) workingState.ignoredActions.RemoveAt(i);
                        }
                        
                        OnIgnoreStatusChanged.Invoke();
                    });
                }
                else
                {
                    evt.menu.AppendAction("Ignore", (x) =>
                    {
                        Undo.RecordObject(workingState, $"Ignored action of id {stateActionAsset.id} from {stateAsset.Label}.");
                        
                        workingState.ignoredActions.Add(new HNSFStateIgnoredAction()
                        {
                            actionId = stateActionAsset.id,
                            stateRef = stateAsset
                        });
                        
                        OnIgnoreStatusChanged.Invoke();
                    });
                }
            }
        }));
    }

    private void PasteAsChildWithChildren(HNSFStateAction actionCopy)
    {
        var copy = actionCopy.Copy();
        copy.id = stateAsset.GenerateValidActionId();
        copy.parent = stateActionAsset;

        int validIdCounter = copy.id+1;
        for (int i = 0; i < copy.children.Length; i++)
        {
            StateEditorHelpers.RegenerateIDs(copy.children[i], ref validIdCounter);
            validIdCounter += 1;
        }

        Undo.RecordObject(stateAsset, "Pasted Action As Child With Children");
        Array.Resize(ref stateActionAsset.children, stateActionAsset.children.Length + 1);
        stateActionAsset.children[^1] = copy;
        
        StateEditorHelpers.ValidateStateIDs(stateAsset);
        
        EditorUtility.SetDirty(stateAsset);
    }

    private void PasteAsChildWithoutChildren(HNSFStateAction actionCopy)
    {
        var copy = actionCopy.Copy();
        copy.id = stateAsset.GenerateValidActionId();
        copy.parent = stateActionAsset;
        copy.children = Array.Empty<HNSFStateAction>();
        
        Undo.RecordObject(stateAsset, "Pasted Action As Child Without Children");
        Array.Resize(ref stateActionAsset.children, stateActionAsset.children.Length + 1);
        stateActionAsset.children[^1] = copy;
        
        StateEditorHelpers.ValidateStateIDs(stateAsset);
        
        EditorUtility.SetDirty(stateAsset);
    }

    private void PasteInPlaceWithoutChildren(HNSFStateAction actionCopy)
    {
        var copy = actionCopy.Copy();
        copy.id = stateActionAsset.id;
        copy.Label = stateActionAsset.Label;
        copy.parent = stateActionAsset.parent;

        copy.children = new HNSFStateAction[stateActionAsset.children.Length];

        for (int i = 0; i < stateActionAsset.children.Length; i++)
        {
            copy.children[i] = stateActionAsset.children[i];
        }
        
        if (stateActionAsset.parent == null)
        {
            var index = Array.IndexOf(stateAsset.data, stateActionAsset);
            if (index == -1) return;

            Undo.RecordObject(stateAsset, "Pasted Actions in Place Without Children");
            stateAsset.data[index] = copy;
        }
        else
        {
            var index = Array.IndexOf(stateActionAsset.parent.children, stateActionAsset);
            if (index == -1) return;
            
            Undo.RecordObject(stateAsset, "Pasted Actions in Place Without Children");
            stateActionAsset.parent.children[index] = copy;
        }
        
        StateEditorHelpers.ValidateStateIDs(stateAsset);
        
        EditorUtility.SetDirty(stateAsset);
    }

    private void PasteInPasteWithChildren(HNSFStateAction actionCopy)
    {
        var copy = actionCopy.Copy();
        copy.id = stateActionAsset.id;
        copy.Label = stateActionAsset.Label;
        copy.parent = stateActionAsset.parent;

        int validIdCounter = stateAsset.GenerateValidActionId();
        for (int i = 0; i < copy.children.Length; i++)
        {
            StateEditorHelpers.RegenerateIDs(copy.children[i], ref validIdCounter);
            validIdCounter += 1;
        }

        if (stateActionAsset.parent == null)
        {
            var index = Array.IndexOf(stateAsset.data, stateActionAsset);
            if (index == -1) return;

            Undo.RecordObject(stateAsset, "Pasted Actions in Place With Children");
            stateAsset.data[index] = copy;
        }
        else
        {
            var index = Array.IndexOf(stateActionAsset.parent.children, stateActionAsset);
            if (index == -1) return;
            
            Undo.RecordObject(stateAsset, "Pasted Actions in Place With Children");
            stateActionAsset.parent.children[index] = copy;
        }
        
        StateEditorHelpers.ValidateStateIDs(stateAsset);
        
        EditorUtility.SetDirty(stateAsset);
    }

    public void UpdateUi()
    {
        var lli = this.Q<LabelListItem>();

        lli.Label.style.color = StateEditorHelpers.IsIgnored(workingState, stateAsset, stateActionAsset) ? Color.gray : Color.white;
        lli.Text.style.color = StateEditorHelpers.IsIgnored(workingState, stateAsset, stateActionAsset) ? Color.gray : Color.white;
    }

    public bool IsIgnored()
    {
        return IsIgnoredByWorkingState() || IsIgnoredInBaseStates();
    }
    
    private bool IsIgnoredByWorkingState()
    {
        return workingState.ignoredActions.Exists((x) =>
            x.stateRef == stateAsset && x.actionId == stateActionAsset.id);
    }

    private bool IsIgnoredInBaseStates()
    {
        var bState = workingState.baseState;
        if (bState == default || !QuantumUnityDB.TryGetGlobalAssetEditorInstance(bState, out var baseState))
            return false;

        bool isIgnored = false;
        while (baseState != null)
        {
            if (baseState.ignoredActions.Exists((x) => x.stateRef == stateAsset && x.actionId == stateActionAsset.id))
            {
                isIgnored = true;
                baseState = null;
                break;
            }

            if (baseState.useBaseState == false)
            {
                baseState = null;
                break;
            }

            if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(baseState.baseState, out baseState))
            {
                baseState = null;
                break;
            }
        }
        return isIgnored;
    }

    private void MoveActionToTop()
    {
        if (stateActionAsset.parent != null)
        {
            var stateActionParent = stateActionAsset.parent;
            int currentIndex = Array.IndexOf(stateActionParent.children, stateActionAsset);
            if (currentIndex <= 0) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action to the top of {stateActionParent.Label}.");

            var temp = stateActionParent.children[currentIndex];
            for (int i = currentIndex; i > 0; i--)
            {
                stateActionParent.children[i] = stateActionParent.children[i - 1];
            }
            stateActionParent.children[0] = temp;
        }
        else
        {
            int currentIndex = Array.IndexOf(stateAsset.data, stateActionAsset);
            if (currentIndex <= 0) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action to the top of State Data{stateAsset.Label}.");
            
            var temp = stateAsset.data[currentIndex];
            for (int i = currentIndex; i > 0; i--)
            {
                stateAsset.data[i] = stateAsset.data[i - 1];
            }
            stateAsset.data[0] = temp;
        }
        
        var siblings = this.parent.Children().ToList();
        var selfIndex = this.parent.IndexOf(this);
        siblings[0].PlaceBehind(this);
        
        EditorUtility.SetDirty(stateAsset);
    }
    
    private void MoveActionUp()
    {
        if (stateActionAsset.parent != null)
        {
            var stateActionParent = stateActionAsset.parent;
            int currentIndex = Array.IndexOf(stateActionParent.children, stateActionAsset);
            if (currentIndex <= 0) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action up 1 in {stateActionParent.Label}.");

            (stateActionParent.children[currentIndex], stateActionParent.children[currentIndex-1]) 
                = (stateActionParent.children[currentIndex-1], stateActionParent.children[currentIndex]);
        }
        else
        {
            int currentIndex = Array.IndexOf(stateAsset.data, stateActionAsset);
            if (currentIndex <= 0) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action up 1 in State {stateAsset.Label}.");
            
            (stateAsset.data[currentIndex], stateAsset.data[currentIndex-1]) 
                = (stateAsset.data[currentIndex-1], stateAsset.data[currentIndex]);
        }

        var siblings = this.parent.Children().ToList();
        var selfIndex = this.parent.IndexOf(this);
        siblings[selfIndex-1].PlaceInFront(this);
        
        EditorUtility.SetDirty(stateAsset);
    }
    
    private void MoveActionDown()
    {
        if (stateActionAsset.parent != null)
        {
            var stateActionParent = stateActionAsset.parent;
            int currentIndex = Array.IndexOf(stateActionParent.children, stateActionAsset);
            if (currentIndex == -1 || currentIndex == stateActionParent.children.Length-1) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action down 1 in {stateActionParent.Label}.");

            (stateActionParent.children[currentIndex], stateActionParent.children[currentIndex+1]) 
                = (stateActionParent.children[currentIndex+1], stateActionParent.children[currentIndex]);
        }
        else
        {
            int currentIndex = Array.IndexOf(stateAsset.data, stateActionAsset);
            if (currentIndex == -1 || currentIndex == stateAsset.data.Length-1) return;
            
            Undo.RecordObject(stateAsset, $"Moved Action down 1 in State {stateAsset.Label}.");
            
            (stateAsset.data[currentIndex], stateAsset.data[currentIndex+1]) 
                = (stateAsset.data[currentIndex+1], stateAsset.data[currentIndex]);
        }

        var siblings = this.parent.Children().ToList();
        var selfIndex = this.parent.IndexOf(this);
        siblings[selfIndex+1].PlaceBehind(this);
        
        EditorUtility.SetDirty(stateAsset);
    }

    private void DeleteItem(HNSFStateAction currentAction)
    {
        Undo.RecordObject(stateAsset, $"Removed item from State Action {stateAsset.Label}");
        
        if (currentAction.parent == null)
        {
            stateAsset.data = stateAsset.data.Where((a, b) => (a != currentAction)).ToArray();
        }
        else
        {
            currentAction.parent.children = currentAction.parent.children.Where((a, b) => (a != currentAction) ).ToArray();
        }
        
        EditorUtility.SetDirty(stateAsset);
    }

    private void WhenItemSelected(HNSFStateAction parentAction, Type type)
    {
        if (type == null) return;
        
        HNSFStateAction ass = (HNSFStateAction)Activator.CreateInstance(type);
        ass.parent = parentAction;
        ass.id = stateAsset.GenerateValidActionId();
        
        Undo.RecordObject(stateAsset, $"Added Action {type.Name} to {stateAsset.name}");
        Array.Resize(ref parentAction.children, parentAction.children.Length+1);
        parentAction.children[^1] = ass;
        
        EditorUtility.SetDirty(stateAsset);
        whenActionChanged?.Invoke();
    }
}
