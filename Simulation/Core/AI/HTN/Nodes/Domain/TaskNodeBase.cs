#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Domain
{
    [Serializable]
    [UseWithGraph(typeof(HTNDomainGraph))]
    public abstract class TaskNodeBase : DomainNodeBase
    {
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0.0f, 0.5f, 0f, 1.0f);
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<int>(OPTION_WEIGHT).WithDisplayName("Weight").WithDefaultValue(1).Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            AddInputOutputExecutionPorts(context);
        }

        protected override void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            base.AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .AsVertical()
                .Build();
            
            /*
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();*/
        }
        
        public virtual ITask Convert()
        {
            return null;
        }
    }
}
#endif