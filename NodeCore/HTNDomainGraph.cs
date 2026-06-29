using System;
using Quantum;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.AI.HTN
{
    [Graph(AssetExtension, GraphOptions.Default)]
    [Serializable]
    public class HTNDomainGraph : Graph
    {
        public const string AssetExtension = "htndomain";
        
        [MenuItem("Assets/Create/HnSF/AI/HTN/Domain Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<HTNDomainGraph>();
        }
    }
}