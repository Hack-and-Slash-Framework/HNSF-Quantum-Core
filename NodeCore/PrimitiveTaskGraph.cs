using System;
using Quantum;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.AI.HTN
{
    [Graph(AssetExtension)]
    [Serializable]
    public class PrimitiveTaskGraph : Graph
    {
        public const string AssetExtension = "htn";
        
        [MenuItem("Assets/Create/HnSF/AI/HTN/Primitive Task Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<PrimitiveTaskGraph>();
        }
    }
}