using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/*
namespace HnSF
{
    [CustomEditor(typeof(HNSFStateSetAsset), true)]
    public class HNSFStateSetAssetEditor : Editor
    {
        [SerializeField] public StateSetEditorWindow currentStateSet;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();
            
            // Default Inspector
            InspectorElement.FillDefaultInspector(inspector, serializedObject, this);

            Button openEditor = new Button();
            openEditor.text = "Open Editor";
            openEditor.clicked += () =>
            {
                var st = (HNSFStateSetAsset)target;
                //st.BuildStateVariablesIDMap();
                currentStateSet = StateSetEditorWindow.OpenWindow(st);
            };
            inspector.Add(openEditor);
            return inspector;
        }
    }
}*/