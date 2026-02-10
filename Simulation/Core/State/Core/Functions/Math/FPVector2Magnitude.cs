using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Magnitude : StateFunctionFP
    {
        public HNSFParamFPVector2 value;
        public bool sqrMagnitude;
        
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var v = value.Resolve(frame, entity, ref stateContext);
            return sqrMagnitude ? v.SqrMagnitude : v.Magnitude;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Magnitude());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Magnitude;
            t.value = value.Clone() as HNSFParamFPVector2;
            t.sqrMagnitude = sqrMagnitude;
            return base.CopyTo(target);
        }
    }
}