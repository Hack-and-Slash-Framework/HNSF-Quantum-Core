#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public abstract class FunctionBase : HTNNodeBase
    {
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 0f, 0.5f, 1.0f);
        }
        
        protected override void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            base.AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
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