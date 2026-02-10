using UnityEditor;
using UnityEngine;
     
public class PopUpAssetInspector : EditorWindow
{
    [SerializeField] Vector2 scrollPos;
    [SerializeField] private Object asset;
    [SerializeField] private Editor assetEditor;
       
    public static PopUpAssetInspector Create(Object asset)
    {
        var window = CreateWindow<PopUpAssetInspector>($"{asset.name} | {asset.GetType().Name}");
        window.asset = asset;
        window.assetEditor = Editor.CreateEditor(asset);
        return window;
    }
     
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUI.enabled = false;
        asset = EditorGUILayout.ObjectField("Asset", asset, asset.GetType(), false);
        GUI.enabled = true;
     
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        assetEditor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
}