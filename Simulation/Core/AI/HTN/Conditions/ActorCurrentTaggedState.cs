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

            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorAI->target, out var gsm)
                && frame.TryFindAsset(gsm->stateAgent.stateSet, out var stateSet)
                && frame.TryFindAsset(gsm->stateAgent.stateData.state, out var currentState))
            {
                foreach (var tag in validTags)
                {
                    if(stateSet.AttemptGetStateByTag(gsm->stateAgent.stateData.moveset, tag) != currentState)
                        continue;
                    return true;
                }
            }
            return false;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(HTNDomainGraph))]
    public unsafe class ConditionActorCurrentTaggedState : ConditionBase
    {
        public const string inValidTags = "ValidTags";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            //AddInputOutputExecutionPorts(context);

            context.AddInputPort(inValidTags)
                .WithDataType<List<AssetRef<Tag>>>()
                .WithDisplayName("Tags")
                .Build();
        }

        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);

            return new Conditions.ActorCurrentTaggedState()
            {
                //Label = label,
                validTags = GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(inValidTags))
            };
        }
    }
}
#endif