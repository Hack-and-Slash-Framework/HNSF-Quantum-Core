#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public abstract class NodeBase : Node
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
            context.AddOption<string>(OPTION_LABEL).WithDisplayName("Label");
        }
        
        protected virtual void AddInputOutputExecutionPorts(Unity.GraphToolkit.Editor.Node.IPortDefinitionContext context)
        {
            
        }
        
        protected virtual HTNFunction ConvertFunctionNode(INode getNode)
        {
            if (getNode is FunctionBase functionNode)
            {
                return functionNode.Convert();
            }
            return null;
        }
    }
}
#endif