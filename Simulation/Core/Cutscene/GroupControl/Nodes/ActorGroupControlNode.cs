#if UNITY_EDITOR
using System;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public abstract class ActorGroupControlNode : Node
    {
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";

        public const string IN_PORT_LABEL = "Label";

        public string testThing = "";
        
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
            
            context.AddInputPort<string>(IN_PORT_LABEL)
                .WithDisplayName("Label")
                .Build();
        }

        public virtual GroupControlAction Convert()
        {
            return null;
        }
    }
}
#endif