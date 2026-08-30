using System;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Param;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Conditions
{
    [Serializable]
    public unsafe partial class ActorCurrentStateFrame : ICondition
    {
        #if QUANTUM_UNITY
        [field: SerializeField]
        #endif
        public string Label { get; set; }

        public ComparisonType comparisonType;
        public HTNParamInt minParam;
        public HTNParamInt maxParam;
        
        public bool IsValid(ref HTNAgentContext context)
        {
            var frame = context.frame;
            
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)) return false;

            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorAI->aiActorRef, out var gsm))
                return false;

            var min = minParam.Resolve(ref context);

            switch (comparisonType)
            {
                case ComparisonType.Inbetween:
                    var max = maxParam.Resolve(ref context);
                    return gsm->stateAgent.stateData.frame >= min && gsm->stateAgent.stateData.frame <= max;
                case ComparisonType.Equals:
                    return gsm->stateAgent.stateData.frame == min;
                case ComparisonType.LessThan:
                    return gsm->stateAgent.stateData.frame < min;
                case ComparisonType.LessThanOrEqualTo:
                    return gsm->stateAgent.stateData.frame <= min;
                case ComparisonType.MoreThan:
                    return gsm->stateAgent.stateData.frame > min;
                case ComparisonType.MoreThanOrEqualTo:
                    return gsm->stateAgent.stateData.frame >= min;
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
    public unsafe class ConditionActorCurrentStateFrame : ConditionBase
    {
        public const string optionComparisonType = "ComparisonType";
        public const string inFrameMin = "FrameMin";
        public const string inFrameMax = "FrameMax";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<ComparisonType>(optionComparisonType)
                .WithDisplayName("Comparison")
                .WithDefaultValue(ComparisonType.Equals)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort(inFrameMin)
                .WithDataType<int>()
                .WithDisplayName("Frame (Min)")
                .Build();
            
            context.AddInputPort(inFrameMax)
                .WithDataType<int>()
                .WithDisplayName("Frame (Max)")
                .Build();
        }

        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);

            return new Conditions.ActorCurrentStateFrame()
            {
                //Label = label,
                minParam = NodeHelper.GetInputPortParam<HTNParamInt, int>(GetInputPortByName(inFrameMin)),
                maxParam = NodeHelper.GetInputPortParam<HTNParamInt, int>(GetInputPortByName(inFrameMax))
            };
        }
    }
}
#endif