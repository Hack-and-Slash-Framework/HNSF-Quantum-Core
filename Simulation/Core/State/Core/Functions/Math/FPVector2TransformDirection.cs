using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2TransformDirection : StateFunctionFPVector2
    {
        public HNSFParamFP rotation;
        public HNSFParamFPVector2 localVector;
        
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var fpQ = rotation.Resolve(frame, entity, ref stateContext);
            var lv = localVector.Resolve(frame, entity, ref stateContext);
            return lv.TransformDirection(fpQ * FP.Deg2Rad);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2TransformDirection());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2TransformDirection;
            t.rotation = rotation.Clone() as HNSFParamFP;
            t.localVector = localVector.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}