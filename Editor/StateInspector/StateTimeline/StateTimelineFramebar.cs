using System;
using System.Linq;
using HnSF;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StateTimelineFramebar : VisualElement
{
    private VisualTreeAsset framebarLabelTemplate;

    private HNSFState topState;
    private HNSFState stateAsset;
    private HNSFStateAction stateActionAsset;
    [SerializeField] private SerializedObject stateSo;
    [SerializeField] private SerializedProperty stateActionSP;

    private ActionRange[] frameCounts;
    
    public StateTimelineFramebar(HNSFState topState, HNSFState state, HNSFStateAction stateAction,  SerializedObject stateSo, SerializedProperty stateActionSP)
    {
        this.topState = topState;
        stateAsset = state;
        stateActionAsset = stateAction;
        this.stateSo = stateSo;
        this.stateActionSP = stateActionSP;
        
        var vte = Resources.Load<VisualTreeAsset>("UXML/main-framebar-bg");
        vte.CloneTree(this);

        var mainBarBG = this.Children().First();
        
        mainBarBG.style.width = new StyleLength((topState.totalFrames + 2) * 20.0f);
        
        framebarLabelTemplate = Resources.Load<VisualTreeAsset>("UXML/main-framebar-label");

        UpdateFramebar();
    }

    public void UpdateFramebar()
    {
        var mainBarBG = this.Children().First();
        
        mainBarBG.Clear();
       
        if (stateActionAsset.frameRanges == null || stateActionAsset.frameRanges.Length == 0) return;
        for (int i = 0; i < stateActionAsset.frameRanges.Length; i++)
        {
            int frStart = StateFrameHelper.ConvertFrame(topState.totalFrames, (int)stateActionAsset.frameRanges[i].start);
            int frEnd = StateFrameHelper.ConvertFrame(topState.totalFrames, (int)stateActionAsset.frameRanges[i].end);
            if(frEnd < frStart) frEnd = frStart;
            int framebarStart = frStart;
            int framebarWidth = frEnd - frStart + 1;
            
            framebarLabelTemplate.CloneTree(mainBarBG);
            var currFramebar = mainBarBG.Children().Last();
            currFramebar.name = mainBarBG.name;
            if (framebarWidth <= 0) currFramebar.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            currFramebar.style.left = 20.0f * framebarStart;
            currFramebar.style.width = new StyleLength(20.0f * (framebarWidth));
            currFramebar.style.backgroundColor = new StyleColor(stateActionAsset.ActionColor);
            currFramebar.Q<Label>().text = (frStart + 1 - frStart).ToString();
            
            int rangeIndex = i;
            currFramebar.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;
                
                var w = PopUpPropertyInspector.Create(stateAsset, stateActionAsset, stateSo, stateActionSP);
            });
        }
    }
}
