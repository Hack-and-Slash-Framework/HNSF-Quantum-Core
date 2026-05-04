using System;
using System.Collections.Generic;
using System.Linq;
using HnSF;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class StateTimelineEditorView : VisualElement
{
    private VisualTreeAsset topbarFrameLabelTemplate;
    
    [SerializeField] public HNSFState stateAsset;
    [SerializeField] public float zoomMultiplier = 1.0f;

    [SerializeField] public static HNSFStateAction actionCopy;
    
    public Color frameZeroColor = new Color(0.1141082f, 0.5f, 0);
    public Color frameInterruptColor = new Color(0.5019608f, 0, 0.009442259f);
    
    public Color[] depthColors = {
        Color.grey,
        new Color(0.4f, 0.4f, 0.4f)
    };

    public Color parentColor = new Color(0.05f, 0.05f, 0.05f);
    
    public static Vector2 lastKnownMousePosition;
    
    [SerializeField] private ContextualMenuManipulator sidebarPanelMenuManipulator = null;
    
    public StateTimelineEditorView()
    {
        topbarFrameLabelTemplate = Resources.Load<VisualTreeAsset>("UXML/topbar-frame-label");
        
        var visualTree = Resources.Load<VisualTreeAsset>("UXML/StateTimelineVisualTree");
        visualTree.CloneTree(this);
        
        ScrollView labelPanel = this.Q<ScrollView>(name: "data-labels");
        ScrollView dataPanel = this.Q<ScrollView>(name: "data-frames");
        VisualElement ve = this.Q<VisualElement>(name: "frame-padding");
        
        // SCROLLING
        labelPanel.verticalScroller.valueChanged += (v) => { dataPanel.verticalScroller.value = v; };

        labelPanel.contentContainer.RegisterCallback<WheelEvent>(@event =>
        {
            labelPanel.verticalScroller.value += @event.delta.y * labelPanel.verticalPageSize;
            @event.StopPropagation();
        });

        dataPanel.contentContainer.RegisterCallback<WheelEvent>(@event =>
        {
            dataPanel.horizontalScroller.value += @event.delta.y * dataPanel.horizontalPageSize;
            @event.StopPropagation();
        });
        
        Button zoomIn = this.Q<Button>(name: "zoom-in");
        Button zoomOut = this.Q<Button>(name: "zoom-out");
        Button refresh = this.Q<Button>(name: "refresh");
        zoomIn.clicked += () =>
        {
            ChangeZoomLevel(2.0f);
        };
        zoomOut.clicked += () =>
        {
            ChangeZoomLevel(0.5f);
        };
        refresh.clicked += UndoRedoPerformed;
        
        Undo.undoRedoPerformed += UndoRedoPerformed;
        this.RegisterCallback<PointerDownEvent>(OnPointerDownEvent, TrickleDown.TrickleDown);
        
    }
    
    private void OnPointerDownEvent(PointerDownEvent evt)
    {
        lastKnownMousePosition = evt.position;
    }

    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= UndoRedoPerformed;
    }

    public void SetStateAsset(HNSFState state)
    {
        //if (stateAsset == state) return;
        stateAsset = state;
        CleanupUI();
        BuildUI();
    }

    public virtual HNSFState[] GetStateTimelineParents(HNSFState startingTimeline)
    {
        void AddStateTimelinesToList(List<HNSFState> timelines, HNSFState currentST)
        {
            var baseState = (HNSFState)QuantumUnityDB.GetGlobalAssetEditorInstance(currentST.baseState.Id);
            if(currentST.useBaseState && currentST.processBaseStateFirst) AddStateTimelinesToList(timelines, baseState);
            timelines.Add(currentST);
            if(currentST.useBaseState && !currentST.processBaseStateFirst) AddStateTimelinesToList(timelines, baseState);
        }
            
        var stateTimelineParents = new List<HNSFState>();

        AddStateTimelinesToList(stateTimelineParents, startingTimeline);

        return stateTimelineParents.ToArray();
    }

    public void ForceRefresh()
    {
        CleanupUI();
        BuildUI();
    }
    
    private void CleanupUI()
    {
        ScrollView sidebarPanel = this.Q<ScrollView>(name: "data-labels");
        var labelsToDelete = sidebarPanel.contentContainer.Query<StateActionLabel>().Build();
        foreach (var container in labelsToDelete)
        {
            sidebarPanel.contentContainer.Remove(container);
        }

        var framebarLabels = this.Query(name: topbarFrameLabelTemplate.name).Build();
        foreach (var fbl in framebarLabels)
        {
            fbl.parent.Remove(fbl);
        }
        
        var framebars = this.Query<StateTimelineFramebar>().Build();
        foreach (var f in framebars)
        {
            f.parent.Remove(f);
        }
    }
    
    public virtual void BuildUI()
    {
        if (stateAsset == null) return;
        
        BuildSideBar();
        BuildFrameCounter();
    }
    
    public virtual void BuildSideBar()
    {
        VisualElement root = this;
        ScrollView sidebarPanel = root.Q<ScrollView>(name: "data-labels");

        if(sidebarPanelMenuManipulator != null) sidebarPanel.RemoveManipulator(sidebarPanelMenuManipulator);
        sidebarPanelMenuManipulator = new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("Add", (x) =>
            {
                var p = GUIUtility.GUIToScreenPoint(lastKnownMousePosition);
                var atm = AdvancedTypeModal.Show(p, TypeCache.GetTypesDerivedFrom(typeof(HNSFStateAction)).Where(p =>
                    (p.IsPublic || p.IsNestedPublic) &&
                    !p.IsAbstract &&
                    !p.IsGenericType
                    && !Attribute.IsDefined(p, typeof(IgnoreActionAttribute))
                    ), 
                    20);
                atm.OnItemSelected += (a) => { WhenItemSelected(null, a); };
            });
            if (actionCopy != null)
            {
                evt.menu.AppendAction("Paste/Without Children", (x) =>
                {
                    var copy = actionCopy.Copy();
                    copy.id = stateAsset.GenerateValidActionId();
                    copy.parent = null;
                    copy.children = Array.Empty<HNSFStateAction>();
                    
                    Undo.RecordObject(stateAsset, $"Added Action {copy.GetType().Name} to {stateAsset.name}");
        
                    Array.Resize(ref stateAsset.data, stateAsset.data.Length+1);
                    stateAsset.data[^1] = copy;

                    EditorUtility.SetDirty(stateAsset);
                    
                    ForceRefresh();
                });
                evt.menu.AppendAction("Paste/With Children", (x) =>
                {
                    var copy = actionCopy.Copy();
                    copy.id = stateAsset.GenerateValidActionId();
                    copy.parent = null;

                    int validIdCounter = copy.id+1;
                    for (int i = 0; i < copy.children.Length; i++)
                    {
                        StateEditorHelpers.RegenerateIDs(copy.children[i], ref validIdCounter);
                        validIdCounter += 1;
                    }
                    
                    Undo.RecordObject(stateAsset, $"Added Action {copy.GetType().Name} to {stateAsset.name}");
        
                    Array.Resize(ref stateAsset.data, stateAsset.data.Length+1);
                    stateAsset.data[^1] = copy;
                    
                    EditorUtility.SetDirty(stateAsset);
                    
                    ForceRefresh();
                });
            }
        });
        sidebarPanel.AddManipulator(sidebarPanelMenuManipulator);
        
        // SIDE LABELS
        var stateChain = GetStateTimelineParents(stateAsset);
        for (int s = 0; s < stateChain.Length; s++)
        {
            var currentStateAssetSo = new SerializedObject(stateChain[s]);
            for (int i = 0; i < stateChain[s].data.Length; i++)
            {
                if (stateChain[s].data[i] == null) continue;
                var currentStateActionSp = currentStateAssetSo.FindProperty("data").GetArrayElementAtIndex(i);
                SidebarCreateLabel(sidebarPanel, stateChain[s], stateChain[s].data[i], currentStateAssetSo, currentStateActionSp);
            }
        }
    }
    
    public virtual void SidebarCreateLabel(ScrollView sidebarPanel, HNSFState stateTimeline, HNSFStateAction stateAction, SerializedObject stateSo, SerializedProperty stateActionSp, int depth = 0)
    {
        var sal = new StateActionLabel(stateAsset, stateTimeline, stateAction, stateSo, stateActionSp);
        sal.OnIgnoreStatusChanged.RemoveAllListeners();
        sal.OnIgnoreStatusChanged.AddListener(UpdateIgnoredStatus);
        sal.SetupContextMenu(stateTimeline, stateAction);
        sal.whenActionChanged += ForceRefresh;
        sidebarPanel.contentContainer.Add(sal);
        
        var thisSideBar = sidebarPanel.contentContainer.Query(name: $"stateActionLabel_{stateTimeline.Guid.ToString()}_{stateAction.id.ToString()}").Build().Last();
        thisSideBar.style.marginLeft = 10 * depth;
        thisSideBar.style.backgroundColor = this.stateAsset != stateTimeline ? parentColor : depthColors[(1+depth) % Mathf.Abs(depthColors.Length)];
        
        CreateFramebar(stateTimeline, stateAction, stateActionSp);
        
        if (stateAction.children == null) return;
        for (int i = 0; i < stateAction.children.Length; i++)
        {
            if (stateAction.children[i] == null) continue;
            SidebarCreateLabel(sidebarPanel, stateTimeline, stateAction.children[i], stateSo, stateActionSp.FindPropertyRelative("children").GetArrayElementAtIndex(i), depth+1);
        }
    }

    private void UpdateIgnoredStatus()
    {
        var sals = this.Query<StateActionLabel>();

        foreach (var sal in sals.Build())
        {
            sal.UpdateUi();
        }
    }

    public virtual StateTimelineFramebar CreateFramebar(HNSFState stateTimeline, HNSFStateAction stateAction, SerializedProperty stateActionSp)
    {
        ScrollView dataPanel = this.Q<ScrollView>(name: "data-frames");
        
        var framebar = new StateTimelineFramebar(stateAsset, stateTimeline, stateAction);
        dataPanel.Add(framebar);
        framebar.TrackPropertyValue(
            stateActionSp.FindPropertyRelative("frameRanges"),
            property => framebar.UpdateFramebar());
        return framebar;
    }

    private void WhenItemSelected(HNSFStateAction parentAction, Type type)
    {
        if (type == null) return;
        
        HNSFStateAction ass = (HNSFStateAction)Activator.CreateInstance(type);
        ass.parent = parentAction;
        ass.id = stateAsset.GenerateValidActionId();

        Undo.RecordObject(stateAsset, $"Added Action {type.Name} to {stateAsset.name}");
        
        if (parentAction == null)
        {
            Array.Resize(ref stateAsset.data, stateAsset.data.Length+1);
            stateAsset.data[^1] = ass;
        }
        else
        {
            Array.Resize(ref parentAction.children, parentAction.children.Length+1);
            parentAction.children[^1] = ass;
        }
        
        EditorUtility.SetDirty(stateAsset);
        
        ForceRefresh();
    }
    
    public virtual void BuildFrameCounter()
    {
        VisualElement root = this;
        //ScrollView labelPanel = root.Q<ScrollView>(name: "data-labels");
        ScrollView dataPanel = root.Q<ScrollView>(name: "data-frames");

        // Frame bar lengths
        var dataBars = dataPanel.contentContainer.Query(className: "frame-bar").Build();
        var dataBar = dataBars.First();
        dataBar.style.width = new StyleLength((stateAsset.totalFrames + 2) * GetFrameWidth());
        
        var soTimeline = new SerializedObject(stateAsset);
        dataBar.Unbind();
        dataBar.TrackPropertyValue(
            soTimeline.FindProperty("totalFrames"),
            property => Debug.Log("Total Frames Changed."));
        
        // TOPBAR //
        
        // Create frame numbers
        for (int i = 0; i < stateAsset.totalFrames + 2; i++)
        {
            topbarFrameLabelTemplate.CloneTree(dataBars.First());
            var thisFrameLabelNumber = dataBars.First().Query(name: topbarFrameLabelTemplate.name).Build().Last();
            Label l = thisFrameLabelNumber.Children().First() as Label;
            l.text = $"{i}";
            if (i == 0) l.style.backgroundColor = frameZeroColor;
            if (i == stateAsset.totalFrames + 1) l.style.backgroundColor = frameInterruptColor;
            thisFrameLabelNumber.style.width = new StyleLength(GetFrameWidth());
        }
    }

    private void UndoRedoPerformed() {
        CleanupUI();
        BuildUI();
    }
    
    public virtual void ChangeZoomLevel(float multi)
    {
        this.zoomMultiplier *= multi;
    }
    
    public virtual float GetFrameWidth()
    {
        return 20.0f; // * zoomMultiplier;
    }
}
