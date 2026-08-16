#if UNITY_EDITOR
using System.Collections.Generic;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.AI.HTN.Param;
using HnSF.core.GroupControl.Nodes;
using Quantum;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    public static class NodeHelper
    {
        public const string OPTION_LABEL = "Label";
        public const string OPTION_EXECUTE_NODE_TYPE = "ExecuteNodeType";
        public const string OPTION_WEIGHT = "Weight";
        
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";
        public const string ConditionsPortName = "ConditionsPort";
        public const string ExecutingConditionsPortName = "ExecutingConditionsPort";
        public const string EffectsPortName = "EffectsPort";
        
        public static T ConvertFunctionNode<T>(IPort getPort) where T : HTNFunction
        {
            var gotNode = getPort?.FirstConnectedPort.GetNode();
            return ConvertFunctionNode<T>(gotNode);
        }
        
        public static T ConvertFunctionNode<T>(INode gotNode) where T : HTNFunction
        {
            if (gotNode is FunctionBase functionNode)
            {
                return functionNode.Convert() as T;
            }
            return null;
        }
        
        public static List<T> ConvertFunctionNodes<T>(INode[] gotNodes) where T : HTNFunction
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
        
        public static List<T> ConvertFunctionNodes<T>(IPort gotPort) where T : HTNFunction
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
        
        /// <summary>
        /// Gets the value of an input port on a node.
        /// <br/><br/>
        /// The value is obtained from (in priority order):<br/>
        /// 1. Connections to the port (variable nodes, constant nodes, wire portals)<br/>
        /// 2. Embedded value on the port<br/>
        /// 3. Default value of the port<br/>
        /// </summary>
        public static T GetInputPortValue<T>(IPort port)
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
                    case IUntypedConversionNode untypedConversionNode:
                        untypedConversionNode.TryGetValue(out value);
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
        
        public static T GetInputPortBattleScriptingParam<T, Q>(IPort port) where T : BattleScriptingParam<Q>, new()
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
        
        public static T GetInputPortParam<T, Q>(IPort port) where T : HTNParam<Q>, new()
        {
            var param = new T
            {
                Source = HTNParamSource.Value
            };

            if (port.IsConnected)
            {
                switch (port.FirstConnectedPort.GetNode())
                {
                    case BlackboardKeyNode blackboardKeyNode:
                        param.Source = HTNParamSource.Blackboard;
                        param.Key = blackboardKeyNode.GetValue();
                        break;
                    case ConfigKeyNode configKeyNode:
                        param.Source = HTNParamSource.Config;
                        param.Key = configKeyNode.GetValue();
                        break;
                    case FunctionBase functionNode:
                        param.Source = HTNParamSource.Function;
                        param.SetFunction(functionNode.Convert());
                        break;
                    case IVariableNode variableNode:
                        param.Source = HTNParamSource.Value;
                        variableNode.Variable.TryGetDefaultValue(out param.DefaultValue);
                        break;
                    case IConstantNode constantNode:
                        param.Source = HTNParamSource.Value;
                        constantNode.TryGetValue(out param.DefaultValue);
                        break;
                    case IUntypedConversionNode untypedConversionNode:
                        param.Source = HTNParamSource.Value;
                        untypedConversionNode.TryGetValue(out param.DefaultValue);
                        break;
                }
            }
            else
            {
                param.Source = HTNParamSource.Value;
                port.TryGetValue(out param.DefaultValue);
            }

            return param;
        }
    }
}
#endif