#if UNITY_EDITOR
using System;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public class StartNode : ActorGroupControlNode
    {
        public const string OPTION_CONTROL_SCRIPT_ASSET = "CopyTarget";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<BattleActorGroupControlScript>(OPTION_CONTROL_SCRIPT_ASSET)
                .WithDisplayName("Copy Target")
                .Build();
        }

        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            // Start is a special node that has no input, so we don't call DefineCommonPorts
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
#endif