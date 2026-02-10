using System;
using Quantum;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl
{
    [Graph(AssetExtension)]
    [Serializable]
    public class ActorGroupScriptGraph : Graph
    {
        public const string AssetExtension = "agsg";
        
        [MenuItem("Assets/Create/HnSF/Actor Group Script Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<ActorGroupScriptGraph>();
        }
    }
}