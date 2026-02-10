using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetHitstun : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Hitstun>(entity, out var hitstun)) return 0;
            return hitstun->value;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetHitstun());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetHitstun;
            return base.CopyTo(target);
        }
    }
}