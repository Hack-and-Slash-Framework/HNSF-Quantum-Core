#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public abstract class OperatorBase : NodeBase
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<NextExecutedNodeType>(OPTION_EXECUTE_NODE_TYPE).WithDisplayName("Next Executed Node Logic");
            context.AddOption<int>(OPTION_WEIGHT).WithDisplayName("Weight").WithDefaultValue(1);
        }

        protected override void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            base.AddInputOutputExecutionPorts(context);
            
            
            context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            
            context.AddInputPort(ConditionsPortName)
                .WithDisplayName("Conditions")
                .WithConnectorUI(PortConnectorUI.Circle)
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
        
        public virtual void ConvertPreconditionNodes(HTNOperatorBase action) 
        {
            List<ICondition> rules = new List<ICondition>();
            var port = GetInputPortByName(ConditionsPortName).firstConnectedPort;
            if (port == null)
            {
                action.preconditions = null;
                return;
            }
            var initialConditionNode = port.GetNode() as ConditionBase;
            if (initialConditionNode == null)
            {
                action.preconditions= null;
                return;
            }
            ConvertRuleNodesRecursive(rules, initialConditionNode);
            rules.Reverse();
            action.preconditions = rules;
        }

        private void ConvertRuleNodesRecursive(List<ICondition> rules, ConditionBase ruleNode)
        {
            rules.Add(ruleNode.Convert());

            var port = ruleNode.GetInputPortByName(ConditionBase.EXECUTION_PORT_DEFAULT_NAME).firstConnectedPort;
            if (port == null)
            {
                return;
            }
            var previousNode = port.GetNode() as ConditionBase;
            if (previousNode == null)
            {
                return;
            }
            ConvertRuleNodesRecursive(rules, previousNode);
        }
    }
}
#endif