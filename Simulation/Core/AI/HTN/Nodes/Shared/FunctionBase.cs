#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.GroupControl.Actions;
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(HTNDomainGraph))]
    public abstract class FunctionBase : Node
    {
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 0f, 0.5f, 1.0f);
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<string>(NodeHelper.OPTION_LABEL)
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort(NodeHelper.EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            
            context.AddOutputPort(NodeHelper.EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("Out")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
        
        public virtual HTNFunction Convert()
        {
            return null;
        }
    }
}
#endif