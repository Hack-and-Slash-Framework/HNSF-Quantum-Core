using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector3TransformDirection : StateFunctionFPVector3
    {
        public HNSFParamFPVector3 directionVector;
        public HNSFParamFPVector3 localVector;
        
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var fpQ = FPQuaternion.LookRotation(directionVector.Resolve(frame, entity, ref stateContext));
            var lv = localVector.Resolve(frame, entity, ref stateContext);
            return lv.TransformDirection(fpQ);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector3TransformDirection());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector3TransformDirection;
            t.directionVector = directionVector.Clone() as HNSFParamFPVector3;
            t.localVector = localVector.Clone() as HNSFParamFPVector3;
            return base.CopyTo(target);
        }
    }
}