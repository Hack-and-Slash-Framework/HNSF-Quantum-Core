using System;
using HnSF.core.AI.HTN.Functions;
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

        public byte minClamp;
        public byte maxClamp;
        
        public override byte Execute(ref HTNAgentContext context)
        {
            return Math.Clamp(inputFunction.Execute(ref context), minClamp, maxClamp);
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

            context.AddInputPort<byte>(inputMin)
                .WithDisplayName("Min")
                .Build();
            
            context.AddInputPort<byte>(inputMax)
                .WithDisplayName("Max")
                .Build();
        }

        public override HTNFunction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            GetInputPortByName(inputMin).TryGetValue<byte>(out var min);
            GetInputPortByName(inputMax).TryGetValue<byte>(out var max);
            
            return new Functions.ClampByte()
            {
                Label = label,
                minClamp = min,
                maxClamp = max,
                inputFunction = ConvertFunctionNode(GetInputPortByName(inputFunction).firstConnectedPort.GetNode()) as HTNFunctionByte
            };
        }
    }
}
#endif