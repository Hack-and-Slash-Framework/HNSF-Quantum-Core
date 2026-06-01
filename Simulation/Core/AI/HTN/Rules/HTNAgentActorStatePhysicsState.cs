using System;
using Quantum;
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    public unsafe partial class HTNAgentActorStateGroundedType : GroupControlRule
    {
        public StateGroundedType state = StateGroundedType.GROUNDED;
        
        public override bool IsValid(Frame frame, EntityRef infoEntityRef)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(infoEntityRef, out var battleActorAI)) return false;

            if (frame.Unsafe.TryGetPointer<BattleActorPhysics>(battleActorAI->target, out var physics))
            {
                return physics->currentGroundedState == state;
            }
            return false;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class HTNAgentIsStateAerialRuleNode : RuleNodeBase
    {
        public const string OPTION_STATE = "State";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<StateGroundedType>(OPTION_STATE)
                .WithDisplayName("State")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlRule Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(OPTION_STATE).TryGetValue<StateGroundedType>(out var state);
            return new HTNAgentActorStateGroundedType()
            {
                Label = label,
                state = state
            };
        }
    }
}
#endif