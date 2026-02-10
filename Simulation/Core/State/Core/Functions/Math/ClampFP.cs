using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class ClampFP : StateFunctionFP
    {
        public HNSFParamFP param;
        public HNSFParamFP minValue;
        public HNSFParamFP maxValue;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return FPMath.Clamp(
                param.Resolve(frame, entity, ref stateContext),
                minValue.Resolve(frame, entity, ref stateContext),
                maxValue.Resolve(frame, entity, ref stateContext));
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new ClampFP());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as ClampFP;
            t.param = param.Clone() as HNSFParamFP;
            t.minValue = minValue.Clone() as HNSFParamFP;
            t.maxValue = maxValue.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}