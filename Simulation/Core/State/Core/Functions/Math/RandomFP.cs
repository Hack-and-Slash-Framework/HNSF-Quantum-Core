using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class RandomFP : StateFunctionFP
    {
        public HNSFParamInt minInclusive;
        public HNSFParamInt maxInclusive;

        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.RNG->NextInclusive(minInclusive.Resolve(frame, entity, ref stateContext),
                maxInclusive.Resolve(frame, entity, ref stateContext));
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new RandomFP());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as RandomFP;
            t.minInclusive = minInclusive.Clone() as HNSFParamInt;
            t.maxInclusive = maxInclusive.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}