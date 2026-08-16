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
    public unsafe partial class CheckWorldState : ICondition
    {
        [field: SerializeField] public string Label { get; set; } = "";
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNParamByte stateID;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNParamByte stateValueMin;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNParamByte stateValueMax;
        public ComparisonType comparisonType = ComparisonType.Equals;
        
        public bool IsValid(ref HTNAgentContext context)
        {
            var currentWorldState = context.frame.ResolveDictionary(context.agent->worldState.current);

            if (!currentWorldState.TryGetValue(stateID.Resolve(ref context), out var stateValue))
                return false;

            var min = stateValueMin.Resolve(ref context);
            var max = min;
            
            switch (comparisonType)
            {
                case ComparisonType.Inbetween:
                    max = stateValueMax.Resolve(ref context);
                    return stateValue >= min && stateValue <= max;
                case ComparisonType.Equals:
                    return stateValue == min;
                case ComparisonType.MoreThan:
                    return stateValue > min;
                case ComparisonType.MoreThanOrEqualTo:
                    return stateValue >= min;
                case ComparisonType.LessThan:
                    return stateValue < min;
                case ComparisonType.LessThanOrEqualTo:
                    return stateValue <= min;
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
    public unsafe class ConditionCheckWorldState : ConditionBase
    {
        public const string optionComparisonType = "ComparisonType";
        public const string inFunctionStateID = "StateID";
        public const string inFunctionStateValueMin = "ValueMin";
        public const string inFunctionStateValueMax = "ValueMax";

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
            //AddInputOutputExecutionPorts(context);

            context.AddInputPort(inFunctionStateID)
                .WithDisplayName("State ID")
                .Build();

            context.AddInputPort(inFunctionStateValueMin)
                .WithDisplayName("Min")
                .Build();

            context.AddInputPort(inFunctionStateValueMax)
                .WithDisplayName("Max")
                .Build();
        }

        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(optionComparisonType).TryGetValue<ComparisonType>(out var comparisonType);

            return new Conditions.CheckWorldState()
            {
                //Label = label,
                comparisonType = comparisonType,
                stateID = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inFunctionStateID)),
                stateValueMin = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inFunctionStateValueMin)),
                stateValueMax = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inFunctionStateValueMax)),
            };
        }
    }
}
#endif