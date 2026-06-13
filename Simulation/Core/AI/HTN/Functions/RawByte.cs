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
        public const string optionValue = "value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<byte>(optionValue)
                .WithDisplayName("Value")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override HTNFunction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            GetNodeOptionByName(optionValue).TryGetValue<byte>(out var value);
            
            return new Functions.RawByte()
            {
                Label = label,
                value = value
            };
        }
    }
}
#endif