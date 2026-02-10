using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2InverseTransformDirection : StateFunctionFPVector2
    {
        public HNSFParamFP rotation;
        public HNSFParamFPVector2 worldVector;
        
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var fpQ = rotation.Resolve(frame, entity, ref stateContext);
            var lv = worldVector.Resolve(frame, entity, ref stateContext);
            return lv.InverseTransformDirection(fpQ * FP.Deg2Rad);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2InverseTransformDirection());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2InverseTransformDirection;
            t.rotation = rotation.Clone() as HNSFParamFP;
            t.worldVector = worldVector.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}