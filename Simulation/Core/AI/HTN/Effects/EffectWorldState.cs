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

namespace HnSF.core.AI.HTN.Effects
{
    [Serializable]
    public class EffectWorldState : IEffect
    {
        public string Label
        {
            get => label;
            set => label = value;
        }

        public EffectType EffectType
        {
            get => effectType;
            set => effectType = value;
        }

        public bool Disable
        {
            get => disable;
            set => disable = value;
        }

        public string label;
        public EffectType effectType;
        public bool disable;

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunctionByte stateID;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNFunctionByte stateValue;
        public bool dirtyWorldState = true;

        public virtual void Apply(ref HTNAgentContext context)
        {
            HTNWorldState.SetState(
                context: ref context,
                state: stateID.Execute(ref context),
                value: stateValue.Execute(ref context),
                setAsDirty: dirtyWorldState,
                e: effectType
            );
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class EffectWorldState : EffectBase
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

        public override IEffect Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(optionEffectType).TryGetValue<EffectType>(out var effectType);
            this.GetNodeOptionByName(optionDirtyWorldState).TryGetValue<bool>(out var dirtyWorldState);
            
            return new Effects.EffectWorldState()
            {
                Label = label,
                EffectType = effectType,
                dirtyWorldState = dirtyWorldState,
                stateID = ConvertFunctionNode(GetInputPortByName(inputStateId).FirstConnectedPort?.GetNode()) as HTNFunctionByte,
                stateValue = ConvertFunctionNode(GetInputPortByName(inputStateValue).FirstConnectedPort?.GetNode()) as HTNFunctionByte,
            };
        }
    }
}
#endif