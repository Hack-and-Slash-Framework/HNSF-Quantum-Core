using System;
using Quantum;
#if QUANTUM_UNITY
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Operators
{
    [Serializable]
    public unsafe partial class OperatorDirtyWorldState : HTNOperatorBase
    {
        public bool value = true;
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            HTNWorldState.SetDirty(ref context, value);
            return HTNTaskStatus.Success;
        }

        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            return HTNTaskStatus.Success;
        }

        public override void OnExit(ref HTNAgentContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class OperatorDirtyWorldStateNode : OperatorBase
    {
        public const string inDirtyWorldState = "DirtyWorldState";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>(inDirtyWorldState)
                .WithDisplayName("Dirty World State?")
                .WithDefaultValue(true)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override HTNOperatorBase Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            GetNodeOptionByName(inDirtyWorldState).TryGetValue<bool>(out var value);
            
            return new Operators.OperatorDirtyWorldState()
            {
                Label = label,
                value = value
            };
        }
    }
}
#endif