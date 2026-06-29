#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.AI.HTN.Param;
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(HTNDomainGraph), typeof(PrimitiveTaskGraph))]
    public class ConditionSetNode : HnSF.Nodes.NodeBase
    {
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";

        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0.5f, 0f, 0f, 1.0f);
        }
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("Out")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
        
        public virtual List<ICondition> ConvertConditionBlocks() 
        {
            List<ICondition> foundConditions = new List<ICondition>();

            foreach (var blockNode in BlockNodes)
            {
                var ruleNode = blockNode as ConditionBase;
                if(ruleNode == null)
                    continue;

                var conversion = ruleNode.Convert();
                if (conversion == null)
                {
                    Debug.LogError($"Got a null condition for node {ruleNode.Title}, skipping.");
                    continue;
                }
                foundConditions.Add(conversion);
            }

            return foundConditions;
        }
    }
}
#endif