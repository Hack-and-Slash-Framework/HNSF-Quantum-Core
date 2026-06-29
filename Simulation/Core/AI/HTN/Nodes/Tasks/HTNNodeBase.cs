#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Param;
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public abstract class HTNNodeBase : HnSF.Nodes.NodeBase
    {
        public const string OPTION_LABEL = "Label";
        public const string OPTION_EXECUTE_NODE_TYPE = "ExecuteNodeType";
        public const string OPTION_WEIGHT = "Weight";
        
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";
        public const string ConditionsPortName = "ConditionsPort";
        public const string ExecutingConditionsPortName = "ExecutingConditionsPort";
        public const string EffectsPortName = "EffectsPort";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(OPTION_LABEL)
                .WithDisplayName("Label")
                .Build();
        }
        
        protected override void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            
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
        
        protected virtual T ConvertFunctionNode<T>(IPort getPort) where T : HTNFunction
        {
            var gotNode = getPort?.FirstConnectedPort.GetNode();
            return ConvertFunctionNode<T>(gotNode);
        }
        
        protected virtual T ConvertFunctionNode<T>(INode gotNode) where T : HTNFunction
        {
            if (gotNode is FunctionBase functionNode)
            {
                return functionNode.Convert() as T;
            }
            return null;
        }
        
        protected virtual List<T> ConvertFunctionNodes<T>(INode[] gotNodes) where T : HTNFunction
        {
            var l = new List<T>();

            foreach (var node in gotNodes)
            {
                var r = ConvertFunctionNode<T>(node);
                if(r == null) continue;
                l.Add(r);
            }
            return l;
        }
        
        protected virtual List<T> ConvertFunctionNodes<T>(IPort gotPort) where T : HTNFunction
        {
            var l = new List<T>();

            var portList = new List<IPort>();
            gotPort.GetConnectedPorts(portList);
            
            foreach (var port in portList)
            {
                var r = ConvertFunctionNode<T>(port.GetNode());
                if(r == null) continue;
                l.Add(r);
            }
            return l;
        }
    }
}
#endif