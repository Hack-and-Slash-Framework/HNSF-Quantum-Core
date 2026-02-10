using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Determinant : StateFunctionFP
    {
        public HNSFParamFPVector2 vectorA;
        public HNSFParamFPVector2 vectorB;
        
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var lvA = vectorA.Resolve(frame, entity, ref stateContext);
            var lvB = vectorB.Resolve(frame, entity, ref stateContext);
            return FPVector2.Determinant(lvA, lvB);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Determinant());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Determinant;
            t.vectorA = vectorA.Clone() as HNSFParamFPVector2;
            t.vectorB = vectorB.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}