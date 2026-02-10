using System;
using HnSF.core.state;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace HnSF
{
    public class StateTimelineEditorWindow : EditorWindow
    {
        [SerializeField] public HNSFState state;

        [OnOpenAsset]
        public static bool OpenGraphAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            if (!(asset is HNSFState hnsfState)) return false;

            var ew = OpenWindow(hnsfState);
            ew.Focus();
            return true;
        }
        
        public static StateTimelineEditorWindow OpenWindow(HNSFState state)
        {
            StateTimelineEditorWindow wnd = CreateWindow<StateTimelineEditorWindow>();
            wnd.titleContent = new GUIContent(String.IsNullOrEmpty(state.Label) ? "State Editor" : state.Label);
            wnd.minSize = new Vector2(900, 500);
            wnd.SetSelection(state);
            return wnd;
        }
        
        public virtual void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            
            StateTimelineEditorView stev = new StateTimelineEditorView();
            stev.style.flexGrow = new StyleFloat(1);
            root.Add(stev);
            SetSelection(state);
        }
        
        public void SetSelection(HNSFState stateAsset)
        {
            var stev = rootVisualElement.Q<StateTimelineEditorView>();
            stev.SetStateAsset(stateAsset);
        }
    }
}