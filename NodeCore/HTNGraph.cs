using System;
using Quantum;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl
{
    [Graph(AssetExtension)]
    [Serializable]
    public class HTNGraph : Graph
    {
        public const string AssetExtension = "htn";
        
        [MenuItem("Assets/Create/HnSF/AI/HTN Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<HTNGraph>();
        }
    }
}