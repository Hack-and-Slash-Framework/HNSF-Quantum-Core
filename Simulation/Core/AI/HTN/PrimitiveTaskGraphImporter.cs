#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Quantum;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace HnSF.core.AI.HTN
{
    [ScriptedImporter(1, PrimitiveTaskGraph.AssetExtension)]
    public class PrimitiveTaskGraphImporter : ScriptedImporter
    {
        [NonSerialized] private int indexCounter = 0;
        [NonSerialized] private Dictionary<HTNNodeBase, int> nodeToIndex = new();
        [NonSerialized] private Dictionary<int, HTNNodeBase> indexToNode = new();
        [NonSerialized] private List<HTNOperatorBase> actions = new();
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<PrimitiveTaskGraph>(ctx.assetPath);
            
            if (graph == null)
            {
                Debug.LogError($"Failed to load Group Script graph asset: {ctx.assetPath}");
                return;
            }
            
            var startNodes = graph.GetNodes().OfType<StartNode>().ToList();
            
            foreach (var startNode in startNodes)
            {
                if(startNode == null) 
                    continue;
                
                startNode.GetNodeOptionByName(StartNode.OPTION_CONTROL_SCRIPT_ASSET)
                    .TryGetValue(out PrimitiveTaskAssetObject gcScript);
                if(gcScript == null)
                    continue;

                BuildForTarget(startNode, gcScript);
            }
        }
        
        private void BuildForTarget(StartNode startNode, PrimitiveTaskAssetObject gcScript)
        {
            actions = new List<HTNOperatorBase>();
            
            // Build Indexes
            indexCounter = 0;
            nodeToIndex.Clear();
            indexToNode.Clear();
            BuildIndexMapRecursive(startNode, skipSelf: true);
            
            // Build Map
            BuildActionList();
            var conditions = startNode.ConvertConditionNodes();
            var executingConditions = startNode.ConvertExecutingConditionNodes();
            var effects = startNode.ConvertEffectNodes();
            
            if (HTNHelpers.EditorActionListsEqual(gcScript.Operators, actions)
                && HTNHelpers.EditorConditionsListsEqual(gcScript.Conditions, conditions)
                && HTNHelpers.EditorConditionsListsEqual(gcScript.ExecutingConditions, executingConditions)
                && HTNHelpers.EditorEffectsListsEqual(gcScript.Effects, effects))
            {
                actions = null;
                return;
            }

            gcScript.node.operators = actions;
            gcScript.node.Conditions = conditions;
            gcScript.node.ExecutingConditions = executingConditions;
            gcScript.node.Effects = effects;
            EditorUtility.SetDirty(gcScript);
            actions = null;
        }
        
        private void BuildActionList()
        {
            for (int i = 0; i < indexCounter; i++)
            {
                if (!indexToNode.TryGetValue(i, out var currentNode))
                {
                    Debug.LogError($"Could not get node from index {i}.");
                    break;
                }
                
                var n = currentNode as OperatorBase;
                
                var runtimeNodes = TranslateNodeModelToRuntimeNodes(n);
                var singleNode = runtimeNodes[0]; // TODO: Support multiple outputs from a single node?
                actions.Add(singleNode);
                
                List<IPort> outputPorts = new List<IPort>();
                var outputPort = currentNode.GetOutputPortByName(Nodes.HTNNodeBase.EXECUTION_PORT_DEFAULT_NAME);
                outputPort.GetConnectedPorts(outputPorts);
                if (outputPorts.Count == 0)
                {
                    singleNode.endExecution = true;
                    continue;
                }

                currentNode.GetNodeOptionByName(Nodes.HTNNodeBase.OPTION_EXECUTE_NODE_TYPE).TryGetValue<NextExecutedNodeType>(out var nextNodeExecuteType);
                singleNode.nextOperatorSelectionType = nextNodeExecuteType;
                
                switch (nextNodeExecuteType)
                {
                    case NextExecutedNodeType.Ordered:
                        singleNode.nextOperatorsOrdered = new int[outputPorts.Count];

                        for (int w = 0; w < outputPorts.Count; w++)
                        {
                            var on = outputPorts[w].GetNode() as Nodes.OperatorBase;
                            if (on == null)
                            {
                                Debug.LogError($"Could not get node from index {w}.");
                                continue;
                            }

                            singleNode.nextOperatorsOrdered[w] = nodeToIndex[on];
                        }
                        break;
                    case NextExecutedNodeType.WeightedRandom:
                        List<WeightedListItem<int>> nextNodeIndexes = new();
                        for (int w = 0; w < outputPorts.Count; w++)
                        {
                            var on = outputPorts[w].GetNode() as Nodes.OperatorBase;
                            if (on == null)
                            {
                                Debug.LogError($"Could not get node from index {w}.");
                                continue;
                            }

                            on.GetNodeOptionByName(Nodes.HTNNodeBase.OPTION_WEIGHT).TryGetValue<int>(out var weight);
                            nextNodeIndexes.Add(new WeightedListItem<int>(nodeToIndex[on], weight));
                        }
                        singleNode.nextOperatorsWeighted = new WeightedList<int>(nextNodeIndexes);
                        break;
                }
            }
        }

        private void BuildIndexMapRecursive(INode currentNode, bool skipSelf = false)
        {
            var n = currentNode as Nodes.HTNNodeBase;
            if (n == null)
            {
                Debug.LogError($"Node {currentNode?.GetType()} in graph is not inherited from NodeBase.");
                return;
            }

            if (skipSelf == false && nodeToIndex.TryAdd(n, indexCounter) != false)
            {
                indexToNode.Add(indexCounter, n);
                indexCounter++;
            }

            List<IPort> outputPorts = new List<IPort>();
            var outputPort = currentNode.GetOutputPortByName(HTNNodeBase.EXECUTION_PORT_DEFAULT_NAME);
            outputPort.GetConnectedPorts(outputPorts);
            if (outputPorts.Count == 0) return;
            
            foreach (var port in outputPorts)
            {
                BuildIndexMapRecursive(port.GetNode());
            }
        }

        static List<HTNOperatorBase> TranslateNodeModelToRuntimeNodes(INode nodeModel)
        {
            var n = nodeModel as Nodes.OperatorBase;
            var returnedNodes = new List<HTNOperatorBase>();

            var rv = n.Convert();
            n.ConvertPreconditionNodes(rv);
            if (rv != null) returnedNodes.Add(rv);
            return returnedNodes;
        }
    }
}
#endif