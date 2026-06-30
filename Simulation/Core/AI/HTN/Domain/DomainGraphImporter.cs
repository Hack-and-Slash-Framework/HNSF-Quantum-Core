#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.AI.HTN.Domain;
using HnSF.core.AI.HTN.Tasks;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace HnSF.core.AI.HTN
{
    [ScriptedImporter(1, HTNDomainGraph.AssetExtension)]
    public class DomainGraphImporter : ScriptedImporter
    {
        [NonSerialized] private int indexCounter = 0;
        [NonSerialized] private Dictionary<DomainNodeBase, int> nodeToIndex = new();
        [NonSerialized] private Dictionary<int, DomainNodeBase> indexToNode = new();
        [NonSerialized] private List<HTNOperatorBase> actions = new();
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<HTNDomainGraph>(ctx.assetPath);
            
            if (graph == null)
            {
                Debug.LogError($"Failed to load domain graph asset: {ctx.assetPath}");
                return;
            }
            
            var startNodes = graph.GetNodes().OfType<StartNode>().ToList();
            
            foreach (var startNode in startNodes)
            {
                if(startNode == null) 
                    continue;
                
                startNode.GetNodeOptionByName(StartNode.OPTION_CONTROL_SCRIPT_ASSET)
                    .TryGetValue(out DomainAssetObject domainAssetObject);
                if(domainAssetObject == null)
                    continue;

                BuildForTarget(startNode, domainAssetObject);
            }
        }
        
        private void BuildForTarget(StartNode startNode, DomainAssetObject gcScript)
        {
            var converted = startNode.Convert();

            if (TreesMatch(gcScript.rootNode, converted))
                return;

            gcScript.rootNode = converted;
            EditorUtility.SetDirty(gcScript);
        }

        private static bool TreesMatch(TaskRoot current, TaskRoot converted)
        {
            if (ReferenceEquals(current, converted))
                return true;

            if (current == null || converted == null)
                return false;

            return string.Equals(
                JsonUtility.ToJson(current),
                JsonUtility.ToJson(converted),
                StringComparison.Ordinal);
        }
    }
}
#endif
