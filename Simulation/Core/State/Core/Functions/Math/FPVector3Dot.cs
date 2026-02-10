using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector3Dot : StateFunctionFP
    {
        public HNSFParamFPVector3 valueA;
        public HNSFParamFPVector3 valueB;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var vA = valueA.Resolve(frame, entity, ref stateContext);
            var vB = valueB.Resolve(frame, entity, ref stateContext);
            return FPVector3.Dot(vA, vB);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector3Dot());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector3Dot;
            t.valueA = valueA.Clone() as HNSFParamFPVector3;
            t.valueB = valueB.Clone() as HNSFParamFPVector3;
            return base.CopyTo(target);
        }
    }
}