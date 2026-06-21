#if UNITY_EDITOR
using System;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    public abstract class NodeBase : Node
    {
        protected virtual void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            
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