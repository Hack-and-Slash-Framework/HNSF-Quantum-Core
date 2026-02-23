#if UNITY_EDITOR
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

            var newActions = new List<GroupControlAction>();
            
            var nextNodeModel = GetNextNode(startNodeModel);
            while (nextNodeModel != null)
            {
                var runtimeNodes = TranslateNodeModelToRuntimeNodes(nextNodeModel);
                newActions.AddRange(runtimeNodes);

                nextNodeModel = GetNextNode(nextNodeModel);
            }

            targetAsset.actions.Clear();
            targetAsset.actions = newActions;
            EditorUtility.SetDirty(targetAsset);
        }

        static List<GroupControlAction> TranslateNodeModelToRuntimeNodes(INode nodeModel)
        {
            var returnedNodes = new List<GroupControlAction>();

            var rv = (nodeModel as ActorGroupControlNode).Convert();
            if (rv != null) returnedNodes.Add(rv);
            return returnedNodes;
        }
        
        /// <summary>
        /// Gets the node that is executed after the given node.
        /// </summary>
        /// <param name="currentNode">The current node</param>
        /// <returns>The next node in the graph</returns>
        public static INode GetNextNode(INode currentNode)
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