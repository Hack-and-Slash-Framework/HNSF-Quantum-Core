#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Functions;
using HnSF.core.GroupControl.Grabbers;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithContext(typeof(ControlNodeBase))]
    public abstract class RuleNodeBase : BlockNode
    {
        public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";
        public const string OPTION_LABEL = "Label";
        
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0.5f, 0f, 0f, 1.0f);
        }
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(OPTION_LABEL).WithDisplayName("Label");
        }

        /// <summary>
        /// Defines common input and output execution ports for all nodes in the Visual Novel Director tool.
        /// </summary>
        /// <param name="scope">The scope to define the node.</param>
        protected virtual void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            
        }

        public virtual GroupControlRule Convert()
        {
            return null;
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
    }
}
#endif