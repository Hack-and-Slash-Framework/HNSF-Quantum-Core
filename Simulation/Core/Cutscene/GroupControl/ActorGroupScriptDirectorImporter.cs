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
            
            var startNodeModel = graph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNodeModel == null) return;
            
            var targetNodeModel = graph.GetNodes().OfType<TargetNode>().FirstOrDefault();
            if (targetNodeModel == null) return;
            
            // Update Asset
            var targetAsset = GetInputPortValue<BattleActorGroupControlScript>(targetNodeModel.GetInputPortByName(TargetNode.IN_PORT_CONTROL_SCRIPT_ASSET));
            if (targetAsset == null) return;

            actions = new List<GroupControlAction>();
            
            // Build Indexes
            indexCounter = 0;
            nodeToIndex.Clear();
            indexToNode.Clear();
            BuildIndexMapRecursive(startNodeModel, skipSelf: true);
            
            // Build Map
            BuildActionList();
            
            // Finished
            targetAsset.actions.Clear();
            targetAsset.actions = actions;
            EditorUtility.SetDirty(targetAsset);
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
            var nextNodePort = outputPort.firstConnectedPort;
            var nextNode = nextNodePort?.GetNode();

            return nextNode;
        }
        
        /// <summary>
        /// Gets the value of an input port on a node.
        /// <br/><br/>
        /// The value is obtained from (in priority order):<br/>
        /// 1. Connections to the port (variable nodes, constant nodes, wire portals)<br/>
        /// 2. Embedded value on the port<br/>
        /// 3. Default value of the port<br/>
        /// </summary>
        public static T GetInputPortValue<T>(IPort port)
        {
            T value = default;

            // If port is connected to another node, get value from connection
            if (port.isConnected)
            {
                switch (port.firstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                    default:
                        break;
                }
            }
            else
            {
                // If port has embedded value, return it.
                // Otherwise, return the default value of the port
                port.TryGetValue(out value);
            }

            return value;
        }
    }
}
#endif