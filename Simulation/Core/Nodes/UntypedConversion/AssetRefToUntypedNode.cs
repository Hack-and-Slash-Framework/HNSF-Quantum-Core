#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.GroupControl;
using Quantum;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(ActorGroupScriptGraph), typeof(HTNDomainGraph))]
    public class AssetRefToUntypedNode : Node, IUntypedConversionNode
    {
        public string Tooltip { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Color DefaultColor { get; set; }
        
        public const string inVariable = "var";
        
        protected void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            context.AddInputPort<AssetRef>(inVariable)
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