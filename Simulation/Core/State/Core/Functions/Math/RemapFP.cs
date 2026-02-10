using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class RemapFP : StateFunctionFP
    {
        public HNSFParamFP param;
        public HNSFParamFP fromMin;
        public HNSFParamFP fromMax;
        public HNSFParamFP toMin;
        public HNSFParamFP toMax;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FP t = FPMath.InverseLerp(
                fromMin.Resolve(frame, entity, ref stateContext),
                fromMax.Resolve(frame, entity, ref stateContext),
                param.Resolve(frame, entity, ref stateContext));
            return FPMath.Lerp(
                toMin.Resolve(frame, entity, ref stateContext),
                toMax.Resolve(frame, entity, ref stateContext),
                t);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new RemapFP());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as RemapFP;
            t.param = param.Clone() as HNSFParamFP;
            t.fromMin = fromMin.Clone() as HNSFParamFP;
            t.fromMax = fromMax.Clone() as HNSFParamFP;
            t.toMin = toMin.Clone() as HNSFParamFP;
            t.toMax = toMax.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}