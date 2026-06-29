#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.GroupControl;
using Photon.Deterministic;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(ActorGroupScriptGraph), typeof(HTNDomainGraph))]
    public class FPToUntypedNode : Node, IUntypedConversionNode
    {
        public const string inVariable = "intVar";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<FP>(inVariable)
                .WithDisplayName("")
                .Build();

            context.AddOutputPort(inVariable)
                .WithDisplayName("")
                .Build();
        }

        public bool TryGetValue<T>(out T value)
        {
            value = NodeHelper.GetInputPortValue<T>(GetInputPortByName(inVariable));
            return true;
        }
    }
}
#endif