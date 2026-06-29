#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.GroupControl;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(ActorGroupScriptGraph), typeof(HTNDomainGraph))]
    public class IntToUntypedNode : Node, IUntypedConversionNode
    {
        public const string inVariableInt = "intVar";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>(inVariableInt)
                .WithDisplayName("")
                .Build();

            context.AddOutputPort(inVariableInt)
                .WithDisplayName("")
                .Build();
        }

        public bool TryGetValue<T>(out T value)
        {
            value = NodeHelper.GetInputPortValue<T>(GetInputPortByName(inVariableInt));
            return true;
        }
    }
}
#endif