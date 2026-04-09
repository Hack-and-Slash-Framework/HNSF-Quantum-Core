#if UNITY_EDITOR
using System;
using HnSF.core.GroupControl.Grabbers;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public abstract class RuleNodeBase : ControlNodeBase
    {
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";
        
        public const string OPTION_LABEL = "Label";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(OPTION_LABEL).WithDisplayName("Label");
        }

        /// <summary>
        /// Defines common input and output execution ports for all nodes in the Visual Novel Director tool.
        /// </summary>
        /// <param name="scope">The scope to define the node.</param>
        protected virtual void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();

            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }

        public virtual GroupControlRule Convert()
        {
            return null;
        }
    }
}
#endif