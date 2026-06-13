using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Conditions
{
    [Serializable]
    public unsafe partial class ActorCurrentTaggedState : ICondition
    {
        public string Label
        {
            get => label;
            set => label = value;
        }

        public string label;

        public List<AssetRef<Tag>> validTags = new();
        
        public bool IsValid(ref HTNAgentContext context)
        {
            var frame = context.frame;
            
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)) return false;

            if (frame.Unsafe.TryGetPointer<BattleActorPhysics>(battleActorAI->target, out var physics))
            {
                //return physics->currentGroundedState == state;
            }
            return false;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class ConditionActorCurrentTaggedState : ConditionBase
    {
        public const string inValidTags = "ValidTags";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort(inValidTags)
                .WithDisplayName("Tags")
                .Build();
        }

        public override ICondition Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            //this.GetNodeOptionByName(OPTION_STATE).TryGetValue<StateGroundedType>(out var state);

            return new Conditions.ActorStateGroundedType()
            {
                Label = label,
                //state = state,
            };
        }
    }
}
#endif