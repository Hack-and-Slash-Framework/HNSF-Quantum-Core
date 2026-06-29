#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Domain;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Param;
using HnSF.core.GroupControl.Actions;
using HnSF.Nodes;
using Quantum;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithContext(typeof(HTNNodeBase), typeof(DomainNodeBase), typeof(ConditionSetNode))]
    public abstract class ConditionBase : BlockNode
    {
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0.5f, 0f, 0f, 1.0f);
        }
        
        public virtual ICondition Convert()
        {
            return null;
        }
        
        protected virtual T ConvertFunctionNode<T>(IPort getPort) where T : HTNFunction
        {
            var gotNode = getPort?.FirstConnectedPort.GetNode();
            return ConvertFunctionNode<T>(gotNode);
        }
        
        protected virtual T ConvertFunctionNode<T>(INode gotNode) where T : HTNFunction
        {
            /*
            if (gotNode is FunctionBase functionNode)
            {
                return functionNode.Convert() as T;
            }*/
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
        
        /// <summary>
        /// Gets the value of an input port on a node.
        /// <br/><br/>
        /// The value is obtained from (in priority order):<br/>
        /// 1. Connections to the port (variable nodes, constant nodes, wire portals)<br/>
        /// 2. Embedded value on the port<br/>
        /// 3. Default value of the port<br/>
        /// </summary>
        public virtual T GetInputPortValue<T>(IPort port)
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
    }
}
#endif