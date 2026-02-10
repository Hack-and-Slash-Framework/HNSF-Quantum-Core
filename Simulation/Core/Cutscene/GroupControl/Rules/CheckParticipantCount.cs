using System;
using Quantum;

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
