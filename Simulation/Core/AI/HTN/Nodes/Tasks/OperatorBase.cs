#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public abstract class OperatorBase : HTNNodeBase
    {
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 0.5f, 0, 1.0f);
        }
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<NextExecutedNodeType>(OPTION_EXECUTE_NODE_TYPE)
                .WithDisplayName("Next Executed Node Logic")
                .Build();
            context.AddOption<int>(OPTION_WEIGHT)
                .WithDisplayName("Weight")
                .WithDefaultValue(1)
                .Build();
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
        
        public virtual HTNOperatorBase Convert()
        {
            return null;
        }
    }
}
#endif