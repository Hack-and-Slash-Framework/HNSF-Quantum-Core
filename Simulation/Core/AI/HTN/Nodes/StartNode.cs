#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public class StartNode : HTNNodeBase
    {
        public const string OPTION_CONTROL_SCRIPT_ASSET = "CopyTarget";

        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 1.0f, 0, 1.0f);
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<PrimitiveTaskAssetObject>(OPTION_CONTROL_SCRIPT_ASSET)
                .WithDisplayName("Copy Target")
                .Build();
        }
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("Operators")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            
            context.AddInputPort(ConditionsPortName)
                .WithDisplayName("Conditions")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            
            context.AddInputPort(ExecutingConditionsPortName)
                .WithDisplayName("Executing Conditions")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            
            context.AddInputPort(EffectsPortName)
                .WithDisplayName("Effects")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
        
        public virtual List<ICondition> ConvertConditionNodes() 
        {
            List<ICondition> conditions = new List<ICondition>();
            var port = GetInputPortByName(ConditionsPortName).FirstConnectedPort;
            if (port == null)
            {
                return conditions;
            }
            var initialConditionNode = port.GetNode() as ConditionBase;
            if (initialConditionNode == null)
            {
                return conditions;
            }
            ConvertConditionNodesRecursive(conditions, initialConditionNode);
            conditions.Reverse();
            return conditions;
        }
        
        public virtual List<ICondition> ConvertExecutingConditionNodes() 
        {
            List<ICondition> conditions = new List<ICondition>();
            var port = GetInputPortByName(ExecutingConditionsPortName).FirstConnectedPort;
            if (port == null)
            {
                return conditions;
            }
            var initialConditionNode = port.GetNode() as ConditionBase;
            if (initialConditionNode == null)
            {
                return conditions;
            }
            ConvertConditionNodesRecursive(conditions, initialConditionNode);
            conditions.Reverse();
            return conditions;
        }

        private void ConvertConditionNodesRecursive(List<ICondition> rules, ConditionBase ruleNode)
        {
            rules.Add(ruleNode.Convert());

            var port = ruleNode.GetInputPortByName(ConditionBase.EXECUTION_PORT_DEFAULT_NAME).FirstConnectedPort;
            if (port == null)
            {
                return;
            }
            var previousNode = port.GetNode() as ConditionBase;
            if (previousNode == null)
            {
                return;
            }
            ConvertConditionNodesRecursive(rules, previousNode);
        }
        
        public virtual List<IEffect> ConvertEffectNodes() 
        {
            List<IEffect> effects = new List<IEffect>();
            var port = GetInputPortByName(EffectsPortName).FirstConnectedPort;
            if (port == null)
            {
                return effects;
            }

            var initialConditionNode = port.GetNode() as EffectBase;
            if (initialConditionNode == null)
            {
                return effects;
            }
            ConvertEffectNodesRecursive(effects, initialConditionNode);
            effects.Reverse();
            return effects;
        }
        
        private void ConvertEffectNodesRecursive(List<IEffect> effects, EffectBase effectNode)
        {
            effects.Add(effectNode.Convert());

            var port = effectNode.GetInputPortByName(EffectBase.EXECUTION_PORT_DEFAULT_NAME).FirstConnectedPort;
            if (port == null)
            {
                return;
            }
            var previousNode = port.GetNode() as EffectBase;
            if (previousNode == null)
            {
                return;
            }
            ConvertEffectNodesRecursive(effects, previousNode);
        }
    }
}
#endif