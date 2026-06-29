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
    public class ConfigKeyNode : Node
    {
        public const string inVariable = "intVar";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(inVariable)
                .WithDisplayName("")
                .Build();

            context.AddOutputPort(inVariable)
                .WithDisplayName("")
                .Build();
        }

        public string GetValue()
        {
            return NodeHelper.GetInputPortValue<string>(GetInputPortByName(inVariable));
        }
    }
}
#endif