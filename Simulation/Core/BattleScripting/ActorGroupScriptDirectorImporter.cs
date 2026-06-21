#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace HnSF.core.GroupControl.Nodes
{
    [ScriptedImporter(1, ActorGroupScriptGraph.AssetExtension)]
    public class ActorGroupScriptDirectorImporter : ScriptedImporter
    {
        [NonSerialized] private int indexCounter = 0;
        [NonSerialized] private Dictionary<ControlNodeBase, int> nodeToIndex = new();
        [NonSerialized] private Dictionary<int, ControlNodeBase> indexToNode = new();
        [NonSerialized] private List<GroupControlAction> actions = new();
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<ActorGroupScriptGraph>(ctx.assetPath);
            
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
                    .TryGetValue(out BattleActorGroupControlScript gcScript);
                if(gcScript == null)
                    continue;

                BuildForTarget(startNode, gcScript);
            }
        }

        private void BuildForTarget(StartNode startNode, BattleActorGroupControlScript gcScript)
        {
            actions = new List<GroupControlAction>();
            
            // Build Indexes
            indexCounter = 0;
            nodeToIndex.Clear();
            indexToNode.Clear();
            BuildIndexMapRecursive(startNode, skipSelf: true);
            
            // Build Map
            BuildActionList();
            
            if (ActionListsEqual(gcScript.actions, actions))
            {
                actions = null;
                return;
            }

            gcScript.actions = actions;
            EditorUtility.SetDirty(gcScript);
            actions = null;
        }

        private static bool ActionListsEqual(IReadOnlyList<GroupControlAction> currentActions, IReadOnlyList<GroupControlAction> generatedActions)
        {
            if (ReferenceEquals(currentActions, generatedActions))
                return true;

            if (currentActions == null || generatedActions == null)
                return false;

            if (currentActions.Count != generatedActions.Count)
                return false;

            for (int i = 0; i < currentActions.Count; i++)
            {
                if (!ActionsEqual(currentActions[i], generatedActions[i]))
                    return false;
            }

            return true;
        }

        private static bool ActionsEqual(GroupControlAction currentAction, GroupControlAction generatedAction)
        {
            if (ReferenceEquals(currentAction, generatedAction))
                return true;

            if (currentAction == null || generatedAction == null)
                return false;

            if (currentAction.GetType() != generatedAction.GetType())
                return false;

            return EditorJsonUtility.ToJson(currentAction) == EditorJsonUtility.ToJson(generatedAction);
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
                
                var n = currentNode as ActorGroupControlNode;
                
                var runtimeNodes = TranslateNodeModelToRuntimeNodes(n);
                var singleNode = runtimeNodes[0]; // TODO: Support multiple outputs from a single node?
                actions.Add(singleNode);
                
                List<IPort> outputPorts = new List<IPort>();
                var outputPort = currentNode.GetOutputPortByName(ActorGroupControlNode.EXECUTION_PORT_DEFAULT_NAME);
                outputPort.GetConnectedPorts(outputPorts);
                if (outputPorts.Count == 0)
                {
                    singleNode.endExecution = true;
                    continue;
                }

                currentNode.GetNodeOptionByName(ActorGroupControlNode.OPTION_EXECUTE_NODE_TYPE).TryGetValue<NextExecutedNodeType>(out var nextNodeExecuteType);
                singleNode.nextExecutedNodeLogic = nextNodeExecuteType;
                
                switch (nextNodeExecuteType)
                {
                    case NextExecutedNodeType.Ordered:
                        singleNode.nextNodesOrdered = new int[outputPorts.Count];

                        for (int w = 0; w < outputPorts.Count; w++)
                        {
                            var on = outputPorts[w].GetNode() as ControlNodeBase;
                            if (on == null)
                            {
                                Debug.LogError($"Could not get node from index {w}.");
                                continue;
                            }

                            singleNode.nextNodesOrdered[w] = nodeToIndex[on];
                        }
                        break;
                    case NextExecutedNodeType.WeightedRandom:
                        List<WeightedListItem<int>> nextNodeIndexes = new();
                        for (int w = 0; w < outputPorts.Count; w++)
                        {
                            var on = outputPorts[w].GetNode() as ControlNodeBase;
                            if (on == null)
                            {
                                Debug.LogError($"Could not get node from index {w}.");
                                continue;
                            }

                            on.GetNodeOptionByName(ActorGroupControlNode.OPTION_WEIGHT).TryGetValue<int>(out var weight);
                            nextNodeIndexes.Add(new WeightedListItem<int>(nodeToIndex[on], weight));
                        }
                        singleNode.nextNodesWeighted = new WeightedList<int>(nextNodeIndexes);
                        break;
                }
            }
        }

        private void BuildIndexMapRecursive(INode currentNode, bool skipSelf = false)
        {
            var n = currentNode as ControlNodeBase;
            if (n == null)
            {
                Debug.LogError("Node in graph is not inherited from ControlNodeBase.");
                return;
            }

            if (skipSelf == false && nodeToIndex.TryAdd(n, indexCounter) != false)
            {
                indexToNode.Add(indexCounter, n);
                indexCounter++;
            }

            List<IPort> outputPorts = new List<IPort>();
            var outputPort = currentNode.GetOutputPortByName(ActorGroupControlNode.EXECUTION_PORT_DEFAULT_NAME);
            outputPort.GetConnectedPorts(outputPorts);
            if (outputPorts.Count == 0) return;
            
            foreach (var port in outputPorts)
            {
                BuildIndexMapRecursive(port.GetNode());
            }
        }

        static List<GroupControlAction> TranslateNodeModelToRuntimeNodes(INode nodeModel)
        {
            var n = nodeModel as ActorGroupControlNode;
            var returnedNodes = new List<GroupControlAction>();

            var rv = n.Convert();
            n.ConvertRuleNodes(rv);
            if (rv != null) returnedNodes.Add(rv);
            return returnedNodes;
        }
        
        /// <summary>
        /// Gets the node that is executed after the given node.
        /// </summary>
        /// <param name="currentNode">The current node</param>
        /// <returns>The next node in the graph</returns>
        public static INode GetFirstNextNode(INode currentNode)
        {
            var outputPort = currentNode.GetOutputPortByName(ActorGroupControlNode.EXECUTION_PORT_DEFAULT_NAME);
            var nextNodePort = outputPort.FirstConnectedPort;
            var nextNode = nextNodePort?.GetNode();

            return nextNode;
        }
    }
}
#endif
