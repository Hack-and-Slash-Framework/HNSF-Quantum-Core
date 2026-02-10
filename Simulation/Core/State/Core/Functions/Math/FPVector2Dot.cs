using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Dot : StateFunctionFP
    {
        public HNSFParamFPVector2 valueA;
        public HNSFParamFPVector2 valueB;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var vA = valueA.Resolve(frame, entity, ref stateContext);
            var vB = valueB.Resolve(frame, entity, ref stateContext);
            return FPVector2.Dot(vA, vB);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Dot());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Dot;
            t.valueA = valueA.Clone() as HNSFParamFPVector2;
            t.valueB = valueB.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}