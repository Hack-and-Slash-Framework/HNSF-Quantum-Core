using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Angle : StateFunctionFP
    {
        public bool signed;
        public HNSFParamFPVector2 lookVectorA;
        public HNSFParamFPVector2 lookVectorB;
        
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var lvA = lookVectorA.Resolve(frame, entity, ref stateContext);
            var lvB = lookVectorB.Resolve(frame, entity, ref stateContext);
            return signed ? (FPVector2.RadiansSigned(lvA, lvB) * FP.Rad2Deg) : FPVector2.Angle(lvA, lvB);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Angle());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Angle;
            t.signed = signed;
            t.lookVectorA = lookVectorA.Clone() as HNSFParamFPVector2;
            t.lookVectorB = lookVectorB.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}