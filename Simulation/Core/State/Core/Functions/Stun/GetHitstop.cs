using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetHitstop : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Hitstop>(entity, out var hitstop)) return 0;
            return hitstop->value;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetHitstop());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            return base.CopyTo(target);
        }
    }
}