using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetParticipantCount : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var gamemodeParticipantsGlobal = frame.Unsafe.GetPointerSingleton<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);
            return participantDataEntities.Count;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetParticipantCount());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetParticipantCount;
            return base.CopyTo(target);
        }
    }
}