using HnSF.core.state;
using HnSF.core.state.actions;
using UnityEditor;
using UnityEngine;
     
public class PopUpPropertyInspector : EditorWindow
{
    [SerializeField] Vector2 scrollPos;
    [SerializeField] private HNSFState stateAsset;
    [SerializeField] private HNSFStateAction stateAction;
    private SerializedObject serializedObject; 
    private SerializedProperty asset;
    [SerializeField] private string assetPropertyPath;
    
    public static PopUpPropertyInspector Create(HNSFState stateAsset, HNSFStateAction stateAction, SerializedObject so, SerializedProperty asset)
    {
        var window = CreateWindow<PopUpPropertyInspector>($"{asset.name} | {asset.GetType().Name}");
        window.stateAsset = stateAsset;
        window.stateAction = stateAction;
        window.serializedObject = so;
        window.asset = asset;
        window.assetPropertyPath = asset.propertyPath;
        stateAction.OnValidate();
        return window;
    }
    
    private void OnGUI()
    {
        if (stateAsset == null)
        {
            Close();
            return;
        }
        if (serializedObject == null)
        {
            serializedObject = new SerializedObject(stateAsset);
            asset = serializedObject.FindProperty(assetPropertyPath);
        }
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.PropertyField(asset);

        if (serializedObject.ApplyModifiedProperties())
        {
            if(stateAction != null) stateAction.OnValidate();
        }
        
        EditorGUILayout.EndScrollView();
    }
}