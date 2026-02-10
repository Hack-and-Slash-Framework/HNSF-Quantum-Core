using System.IO;
using System.Linq;
using Quantum;
using UnityEditor;
using UnityEngine;

public static class FileCreationHelpers
{
    /*
    [MenuItem("Assets/Create/HnSF/StateFunction", priority = -10000)]
    private static void SearchAndCreateForStateFunction()
    {
        string folderGuid = Selection.assetGUIDs[0];
        string projectFolderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

        var atm = AdvancedTypeModal.Show(Vector2.zero, 
            TypeCache.GetTypesDerivedFrom(typeof(HNSFStateFunction)).Where(p =>
                (p.IsPublic || p.IsNestedPublic) &&
                !p.IsAbstract &&
                !p.IsGenericType), 
            20);
        atm.OnItemSelected += (a) =>
        {
            var sfa = ScriptableObject.CreateInstance(a);
            
            var p = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(projectFolderPath + $"\\{sfa.GetType().Name}.asset");
            AssetDatabase.CreateAsset(sfa, p);
            AssetDatabase.SaveAssets();
        };
    }
    
    [MenuItem("Assets/Create/HnSF/StateDecision")]
    private static void SearchAndCreateForStateDecision()
    {
        string folderGuid = Selection.assetGUIDs[0];
        string projectFolderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

        var atm = AdvancedTypeModal.Show(Vector2.zero, 
            TypeCache.GetTypesDerivedFrom(typeof(HNSFStateDecision)).Where(p =>
                (p.IsPublic || p.IsNestedPublic) &&
                !p.IsAbstract &&
                !p.IsGenericType), 
            20);
        atm.OnItemSelected += (a) =>
        {
            var sfa = ScriptableObject.CreateInstance(a);
            
            var p = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(projectFolderPath + $"\\{sfa.GetType().Name}.asset");
            AssetDatabase.CreateAsset(sfa, p);
            AssetDatabase.SaveAssets();
        };
    }
    
    [MenuItem("Assets/Create/HnSF/StateAction")]
    private static void SearchAndCreateForStateAction()
    {
        string folderGuid = Selection.assetGUIDs[0];
        string projectFolderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

        var atm = AdvancedTypeModal.Show(Vector2.zero, 
            TypeCache.GetTypesDerivedFrom(typeof(HNSFStateAction)).Where(p =>
                (p.IsPublic || p.IsNestedPublic) &&
                !p.IsAbstract &&
                !p.IsGenericType), 
            20);
        atm.OnItemSelected += (a) =>
        {
            var sfa = ScriptableObject.CreateInstance(a);
            
            var p = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(projectFolderPath + $"\\{sfa.GetType().Name}.asset");
            AssetDatabase.CreateAsset(sfa, p);
            AssetDatabase.SaveAssets();
        };
    }*/
}
