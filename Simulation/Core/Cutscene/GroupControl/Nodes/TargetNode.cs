#if UNITY_EDITOR
using System;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class TargetNode : ActorGroupControlNode
    {
        public const string IN_PORT_CONTROL_SCRIPT_ASSET = "CopyTarget";
        
        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            context.AddInputPort<BattleActorGroupControlScript>(IN_PORT_CONTROL_SCRIPT_ASSET)
                .WithDisplayName("Copy Target")
                .Build();
            /*
            // Start is a special node that has no input, so we don't call DefineCommonPorts
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();.*/
        }
        
        /*
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<BattleActorGroupControlScript>(IN_PORT_CONTROL_SCRIPT_ASSET)
                .WithDisplayName("Copy Target")
                .Build();
        }*/
    }
}
#endif