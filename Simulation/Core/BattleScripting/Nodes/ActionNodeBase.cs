#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.core.GroupControl.Grabbers;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public abstract class ActorGroupControlNode : ControlNodeBase
    {
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";

        public const string OPTION_LABEL = "Label";
        
        public const string OPTION_WEIGHT = "Weight";
        
        public const string OPTION_EXECUTE_NODE_TYPE = "ExecuteNodeType";

        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 0.5f, 0, 1.0f);
        }
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(OPTION_LABEL).WithDisplayName("Label");
            context.AddOption<NextExecutedNodeType>(OPTION_EXECUTE_NODE_TYPE).WithDisplayName("Next Executed Node Logic");
            context.AddOption<int>(OPTION_WEIGHT).WithDisplayName("Weight").WithDefaultValue(1);
        }

        /// <summary>
        /// Defines common input and output execution ports for all nodes in the Visual Novel Director tool.
        /// </summary>
        /// <param name="scope">The scope to define the node.</param>
        protected void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            
            context.AddInputPort(IN_PORT_CONDITIONS)
                .WithDisplayName("Conditions")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }

        public virtual GroupControlAction Convert()
        {
            return null;
        }
    }
}
#endif