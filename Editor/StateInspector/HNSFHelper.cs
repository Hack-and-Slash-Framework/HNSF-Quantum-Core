using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Quantum;
using UnityEditor;

public static class HNSFHelper
{
    /*
    public static void CreateStateAssetFolder(HNSFState stateAsset)
    {
        var stateAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(stateAsset));
        var stateActionFolder = Path.Combine(stateAssetPath!, "StateActions");
        var stateActionNameFolder = Path.Combine(stateActionFolder, stateAsset.name);
        
        if (!AssetDatabase.IsValidFolder(stateActionFolder))
            AssetDatabase.CreateFolder(stateAssetPath, "StateActions");

        if (!AssetDatabase.IsValidFolder(stateActionNameFolder))
            AssetDatabase.CreateFolder(stateActionFolder, stateAsset.name);

        AssetDatabase.SaveAssets();
    }
    
    public static string CreateStateActionFolder(HNSFState stateAsset, HNSFStateAction stateAction)
    {
        var stateAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(stateAsset));
        var stateActionFolder = Path.Combine(stateAssetPath!, "StateActions");
        var stateActionNameFolder = Path.Combine(stateActionFolder, stateAsset.name);
        var stateActionAssetPath = Path.Combine(stateActionNameFolder, stateAction.name + ".asset");
        
        CreateStateAssetFolder(stateAsset);
        
        return AssetDatabase.GenerateUniqueAssetPath(stateActionAssetPath);
    }*/
}
