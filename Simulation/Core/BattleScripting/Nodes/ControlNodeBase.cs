#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.core.GroupControl.Grabbers;
using Quantum;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public class ControlNodeBase : Node
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
                }
            }
            else
            {
                param.Source = HNSFParamSource.Value;
                port.TryGetValue(out param.DefaultValue);
            }

            return param;
        }
        
        /// <summary>
        /// Gets the value of an input port on a node.
        /// <br/><br/>
        /// The value is obtained from (in priority order):<br/>
        /// 1. Connections to the port (variable nodes, constant nodes, wire portals)<br/>
        /// 2. Embedded value on the port<br/>
        /// 3. Default value of the port<br/>
        /// </summary>
        public T GetInputPortValue<T>(IPort port)
        {
            T value = default;

            // If port is connected to another node, get value from connection
            if (port.IsConnected)
            {
                switch (port.FirstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.Variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                    default:
                        break;
                }
            }
            else
            {
                // If port has embedded value, return it.
                // Otherwise, return the default value of the port
                port.TryGetValue(out value);
            }

            return value;
        }
    }
}
#endif