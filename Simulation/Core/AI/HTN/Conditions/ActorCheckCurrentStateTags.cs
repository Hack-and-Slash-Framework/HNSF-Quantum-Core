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
    public unsafe partial class ActorCheckCurrentStateTags : ICondition
    {
        public string Label
        {
            get => label;
            set => label = value;
        }

        public string label;

        public List<AssetRef<Tag>> validTags = new();
        public List<AssetRef<Tag>> invalidTags = new();
        
        public bool IsValid(ref HTNAgentContext context)
        {
            var frame = context.frame;
            
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)) return false;
            
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorAI->aiActorRef, out var gsm)
                && frame.TryFindAsset(gsm->stateAgent.stateData.state, out var currentState))
            {
                bool valid = false;
                foreach (var validTag in validTags)
                {
                    if (currentState.allTags.Contains(validTag))
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid) return false;

                foreach (var invalidTag in invalidTags)
                {
                    if (currentState.allTags.Contains(invalidTag))
                        return false;
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
    public unsafe class ConditionActorCheckCurrentStateTags : ConditionBase
    {
        public const string inValidTags = "ValidTags";
        public const string inInvalidTags = "InvalidTags";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort(inValidTags)
                .WithDataType<List<AssetRef<Tag>>>()
                .WithDisplayName("Valid Tags")
                .Build();
            
            context.AddInputPort(inInvalidTags)
                .WithDataType<List<AssetRef<Tag>>>()
                .WithDisplayName("Invalid Tags")
                .Build();
        }

        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);

            return new Conditions.ActorCheckCurrentStateTags()
            {
                //Label = label,
                validTags = GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(inValidTags)),
                invalidTags = GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(inInvalidTags)),
            };
        }
    }
}
#endif