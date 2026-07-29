#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.GroupControl;
using Photon.Deterministic;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(ActorGroupScriptGraph), typeof(HTNDomainGraph))]
    public class IntListToUntypedNode : Node, IUntypedConversionNode
    {
        public const string inVariableInt = "intVar";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<List<int>>(inVariableInt)
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