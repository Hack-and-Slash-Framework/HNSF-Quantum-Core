using HnSF.core.state;
using UnityEditor;
using UnityEngine;
     
public class PopUpPropertyInspector : EditorWindow
{
    [SerializeField] Vector2 scrollPos;
    [SerializeField] private HNSFState stateAsset;
    private SerializedObject serializedObject; 
    private SerializedProperty asset;
    [SerializeField] private string assetPropertyPath;
    
    public static PopUpPropertyInspector Create(HNSFState stateAsset, SerializedObject so, SerializedProperty asset)
    {
        var window = CreateWindow<PopUpPropertyInspector>($"{asset.name} | {asset.GetType().Name}");
        window.stateAsset = stateAsset;
        window.serializedObject = so;
        window.asset = asset;
        window.assetPropertyPath = asset.propertyPath;
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
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.EndScrollView();
    }
}