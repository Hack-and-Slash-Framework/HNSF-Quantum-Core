using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetGenericIntTyping : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.Unsafe.TryGetPointer<AssignedIntTypingGeneric>(entity, out var aitg) ? aitg->typing : 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetGenericIntTyping());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetGenericIntTyping;
            return base.CopyTo(target);
        }
    }
}