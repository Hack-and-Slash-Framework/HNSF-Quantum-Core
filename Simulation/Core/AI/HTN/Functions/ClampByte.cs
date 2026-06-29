using System;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Param;
using HnSF.Nodes;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif
#if UNITY_EDITOR
using HnSF.core.AI.HTN.Effects;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Functions
{
    [Serializable]
    public class ClampByte : HTNFunctionByte
    {
        [field: SerializeField] public string Label { get; set; } = "";

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunctionByte inputFunction;

        public HTNParamByte minClamp;
        public HTNParamByte maxClamp;
        
        public override byte Execute(ref HTNAgentContext context)
        {
            return Math.Clamp(inputFunction.Execute(ref context), minClamp.Resolve(ref context), maxClamp.Resolve(ref context));
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(HTNDomainGraph))]
    public unsafe class FunctionClampByte : FunctionBase
    {
        public const string inputFunction = "InputFunction";
        public const string inputMin = "MinValue";
        public const string inputMax = "MaxValue";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort(inputFunction)
                .WithDisplayName("State ID")
                .Build();

            context.AddInputPort(inputMin)
                .WithDisplayName("Min")
                .Build();
            
            context.AddInputPort(inputMax)
                .WithDisplayName("Max")
                .Build();
        }

        public override HTNFunction Convert()
        {
            this.GetNodeOptionByName(NodeHelper.OPTION_LABEL).TryGetValue<string>(out var label);
            
            return new Functions.ClampByte()
            {
                Label = label,
                minClamp = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputMin)),
                maxClamp = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputMax)),
                inputFunction = NodeHelper.ConvertFunctionNode<HTNFunctionByte>(GetInputPortByName(inputFunction))
            };
        }
    }
}
#endif