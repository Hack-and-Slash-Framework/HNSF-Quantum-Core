using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class ClampInt : StateFunctionInt
    {
        public HNSFParamInt param;
        public HNSFParamInt minValue;
        public HNSFParamInt maxValue;
    
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return FPMath.Clamp(
                param.Resolve(frame, entity, ref stateContext),
                minValue.Resolve(frame, entity, ref stateContext),
                maxValue.Resolve(frame, entity, ref stateContext));
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new ClampInt());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as ClampInt;
            t.param = param.Clone() as HNSFParamInt;
            t.minValue = minValue.Clone() as HNSFParamInt;
            t.maxValue = maxValue.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}