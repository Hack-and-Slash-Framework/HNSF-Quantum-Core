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
    public class RawByte : HTNFunctionByte
    {
        public string Label
        {
            get => label;
            set => label = value;
        }

        public byte value;
        
        public override byte Execute(ref HTNAgentContext context)
        {
            return value;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class FunctionRawByte : FunctionBase
    {
        public const string inValue = "Raw";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<int>(inValue)
                .WithDisplayName("Value")
                .WithDefaultValue(0)
                .Build();
        }

        public override HTNFunction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            
            return new Functions.RawByte()
            {
                Label = label,
                value = (byte)GetInputPortValue<int>(GetInputPortByName(inValue))
            };
        }
    }
}
#endif