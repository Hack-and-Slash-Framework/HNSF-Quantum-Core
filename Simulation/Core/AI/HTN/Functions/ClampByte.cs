using System;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Param;
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
        public string Label
        {
            get => label;
            set => label = value;
        }

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
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class FunctionClampByte : FunctionBase
    {
        public const string inputFunction = "InputFunction";
        public const string inputMin = "MinValue";
        public const string inputMax = "MaxValue";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

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
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            
            return new Functions.ClampByte()
            {
                Label = label,
                minClamp = GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputMin)),
                maxClamp = GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputMax)),
                inputFunction = ConvertFunctionNode<HTNFunctionByte>(GetInputPortByName(inputFunction))
            };
        }
    }
}
#endif