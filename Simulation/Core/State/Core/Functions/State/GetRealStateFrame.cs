using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetRealStateFrame : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out HNSFStateAgent* sa)) return 0;
            return sa->stateData.realFrame;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetRealStateFrame());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            return base.CopyTo(target);
        }
    }
}

