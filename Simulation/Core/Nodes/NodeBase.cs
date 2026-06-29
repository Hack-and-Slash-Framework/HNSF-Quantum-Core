#if UNITY_EDITOR
using System;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    public abstract class NodeBase : ContextNode
    {
        protected virtual void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            
        }
    }
}
#endif