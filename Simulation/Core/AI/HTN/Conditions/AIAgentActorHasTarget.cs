using System;
using Quantum;
using Quantum.Physics2D;
using HnSF.core.AI.HTN.Conditions;
#if QUANTUM_UNITY
using UnityEngine;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Conditions
{
    [Serializable]
    public unsafe partial class AIAgentActorHasTarget : ICondition
    {
        [field: SerializeField] public string Label { get; set; } = "";
        public bool inverse;
        
        public bool IsValid(ref HTNAgentContext context)
        {
            var frame = context.frame;
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)
                || !frame.Unsafe.TryGetPointer<EntityTargeting>(battleActorAI->target, out var targeting)) return false;
            var result = targeting->target != default && frame.Exists(battleActorAI->target);
            if (inverse) result = !result;
            return result;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    internal class AIAgentHasTargetNode : ConditionBase
    {
        public const string optionInverse = "inverse";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<bool>(optionInverse)
                .WithDisplayName("Inverse?")
                .WithDefaultValue(false)
                .Build();
        }
        
        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            GetNodeOptionByName(optionInverse).TryGetValue<bool>(out var inverse);
            
            return new Conditions.AIAgentActorHasTarget()
            {
                inverse = inverse,
            };
        }
    }
}
#endif