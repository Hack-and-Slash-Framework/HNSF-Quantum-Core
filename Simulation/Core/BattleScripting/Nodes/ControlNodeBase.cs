#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Param;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.core.GroupControl.Grabbers;
using HnSF.Nodes;
using Quantum;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public class ControlNodeBase : HnSF.Nodes.NodeBase
    {
        public const string IN_PORT_CONDITIONS = "Conditions";
        
        public virtual void ConvertRuleNodes(GroupControlAction action) 
        {
            List<GroupControlRule> rules = new List<GroupControlRule>();
            var port = GetInputPortByName(IN_PORT_CONDITIONS).FirstConnectedPort;
            if (port == null)
            {
                action.rules = null;
                return;
            }
            var initialRuleNode = port.GetNode() as RuleNodeBase;
            if (initialRuleNode == null)
            {
                action.rules = null;
                return;
            }
            ConvertRuleNodesRecursive(rules, initialRuleNode);
            rules.Reverse();
            action.rules = rules.ToArray();
        }

        private void ConvertRuleNodesRecursive(List<GroupControlRule> rules, RuleNodeBase ruleNode)
        {
            rules.Add(ruleNode.Convert());

            var port = ruleNode.GetInputPortByName(RuleNodeBase.EXECUTION_PORT_DEFAULT_NAME).FirstConnectedPort;
            if (port == null)
            {
                return;
            }
            var previousNode = port.GetNode() as RuleNodeBase;
            if (previousNode == null)
            {
                return;
            }
            ConvertRuleNodesRecursive(rules, previousNode);
        }
        
        protected virtual T ConvertFunctionNode<T>(IPort getPort) where T : GroupControlFunction
        {
            var gotNode = getPort?.FirstConnectedPort.GetNode();
            return ConvertFunctionNode<T>(gotNode);
        }
        
        protected virtual T ConvertFunctionNode<T>(INode gotNode) where T : GroupControlFunction
        {
            if (gotNode is FunctionNodeBase functionNode)
            {
                return functionNode.Convert() as T;
            }
            return null;
        }
        
        protected virtual List<T> ConvertFunctionNodes<T>(INode[] gotNodes) where T : GroupControlFunction
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
        
        protected virtual List<T> ConvertFunctionNodes<T>(IPort gotPort) where T : GroupControlFunction
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

        public T GetInputPortParam<T, Q>(IPort port) where T : BattleScriptingParam<Q>, new()
        {
            var param = new T
            {
                Source = HNSFParamSource.Value
            };

            if (port.IsConnected)
            {
                switch (port.FirstConnectedPort.GetNode())
                {
                    case FunctionNodeBase functionNode:
                        param.Source = HNSFParamSource.Function;
                        param.SetFunction(functionNode.Convert());
                        break;
                    case IVariableNode variableNode:
                        param.Source = HNSFParamSource.Value;
                        variableNode.Variable.TryGetDefaultValue(out param.DefaultValue);
                        break;
                    case IConstantNode constantNode:
                        param.Source = HNSFParamSource.Value;
                        constantNode.TryGetValue(out param.DefaultValue);
                        break;
                    case IUntypedConversionNode untypedConversionNode:
                        param.Source = HNSFParamSource.Value;
                        untypedConversionNode.TryGetValue(out param.DefaultValue);
                        break;
                }
            }
            else
            {
                param.Source = HNSFParamSource.Value;
                port.TryGetValue(out param.DefaultValue);
            }

            return param;
        }
    }
}
#endif