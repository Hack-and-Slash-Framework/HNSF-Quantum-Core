#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Domain
{
    [Serializable]
    [UseWithGraph(typeof(HTNDomainGraph))]
    public class StartNode : DomainNodeBase
    {
        public const string OPTION_CONTROL_SCRIPT_ASSET = "CopyTarget";

        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0, 1.0f, 0, 1.0f);
        }
        
        public virtual TaskRoot Convert()
        {
            var task = new TaskRoot();
            var subtasks = new List<ITask>();
            var nextPorts = new List<IPort>();
            task.Weight = 1;
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

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<DomainAssetObject>(OPTION_CONTROL_SCRIPT_ASSET)
                .WithDisplayName("Copy Target")
                .Build();
        }
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("Sub Task")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .AsVertical()
                .Build();
        }
        
        public virtual List<ICondition> ConvertExecutingConditionNodes()
        {
            Debug.LogError("Not implemented executing condition nodes.");
            return null;
            /*
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
            return conditions;*/
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