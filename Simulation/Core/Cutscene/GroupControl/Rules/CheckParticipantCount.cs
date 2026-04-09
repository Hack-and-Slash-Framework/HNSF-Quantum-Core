using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    public unsafe partial class CheckParticipantCount : GroupControlRule
    {
        public enum ComparisonType
        {
            Equals,
            MoreThan,
            MoreThanOrEqualTo,
            LessThan,
            LessThanOrEqualTo,
        }

        public ComparisonType comparison;
        public int compareTo;
        
        public override bool IsValid(Frame frame, EntityRef infoEntityRef)
        {
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);

            switch (comparison)
            {
                case ComparisonType.Equals:
                    return participantDataEntities.Count == compareTo;
                case ComparisonType.MoreThan:
                    return participantDataEntities.Count > compareTo;
                case ComparisonType.MoreThanOrEqualTo:
                    return participantDataEntities.Count >= compareTo;
                case ComparisonType.LessThan:
                    return participantDataEntities.Count < compareTo;
                case ComparisonType.LessThanOrEqualTo:
                    return participantDataEntities.Count <= compareTo;
                default:
                    return false;
            }
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class CheckParticipantCountRuleNode : RuleNodeBase
    {
        public const string OPTION_COMPARISONTYPE = "ComparisonType";
        public const string OPTION_COMPARETO = "CompareTo";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<CheckParticipantCount.ComparisonType>(OPTION_COMPARISONTYPE).WithDefaultValue(CheckParticipantCount.ComparisonType.Equals);
            context.AddOption<int>(OPTION_COMPARETO).WithDefaultValue(0);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlRule Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            this.GetNodeOptionByName(OPTION_COMPARISONTYPE).TryGetValue<CheckParticipantCount.ComparisonType>(out var comparisonType);
            this.GetNodeOptionByName(OPTION_COMPARETO).TryGetValue<int>(out var compareTo);
            return new CheckParticipantCount()
            {
                Label = label,
                comparison = comparisonType,
                compareTo = compareTo
            };
        }
    }
}
#endif