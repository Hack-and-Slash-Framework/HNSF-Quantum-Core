using System;
using HnSF.core.AI.HTN.Functions;
using Quantum;
using UnityEngine;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.AI.HTN.Effects;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Operators
{
    [Serializable]
    public unsafe partial class OperatorEffectWorldState : HTNOperatorBase
    {
        public EffectType effectType;
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunctionByte stateID;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunctionByte stateValue;
        public bool dirtyWorldState = true;
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            HTNWorldState.SetState(
                context: ref context,
                state: stateID.Execute(ref context),
                value: stateValue.Execute(ref context),
                setAsDirty: dirtyWorldState,
                e: effectType
            );
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
    public unsafe class OperatorEffectWorldStateNode : OperatorBase
    {
        public const string optionEffectType = "EffectType";
        public const string optionDirtyWorldState = "DirtyWorldState";
        public const string inputStateId = "StateID";
        public const string inputStateValue = "StateValue";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<EffectType>(optionEffectType)
                .WithDisplayName("Effect Type")
                .WithDefaultValue(EffectType.PlanAndExecute)
                .Build();

            context.AddOption<bool>(optionDirtyWorldState)
                .WithDisplayName("Dirty World State?")
                .WithDefaultValue(true)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort(inputStateId)
                .WithDisplayName("State ID Function")
                .Build();

            context.AddInputPort(inputStateValue)
                .WithDisplayName("State Value Function")
                .Build();
        }

        public override HTNOperatorBase Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(optionEffectType).TryGetValue<EffectType>(out var effectType);
            this.GetNodeOptionByName(optionDirtyWorldState).TryGetValue<bool>(out var dirtyWorldState);
            
            
            return new Operators.OperatorEffectWorldState()
            {
                Label = label,
                effectType = effectType,
                dirtyWorldState = dirtyWorldState,
                stateID = ConvertFunctionNode<HTNFunctionByte>(GetInputPortByName(inputStateId)),
                stateValue = ConvertFunctionNode<HTNFunctionByte>(GetInputPortByName(inputStateValue)),
            };
        }
    }
}
#endif