#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    public class RandomSelectorNode : TaskNodeBase
    {
        protected override void AddInputOutputExecutionPorts(IPortDefinitionContext context)
        {
            base.AddInputOutputExecutionPorts(context);
            
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .AsVertical()
                .Build();
        }
        
        public override ITask Convert()
        {
            var task = new RandomSelector();
            var subtasks = new List<ITask>();
            var nextPorts = new List<IPort>();
            task.Weight = GetWeight();
            
            GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME).GetConnectedPorts(nextPorts);

            foreach (var nextPort in nextPorts)
            {
                var node = nextPort.GetNode();
                var tn = node as TaskNodeBase;
                if(tn == null)
                    continue;
                var conversion = tn.Convert();
                if(conversion == null)
                    continue;
                subtasks.Add(conversion);
            }

            task.Conditions = ConvertConditionBlocks();
            task.subtasks = subtasks;
            return task;
        }
    }
}
#endif