using Photon.Deterministic;
using Quantum;
using UnityEngine.Serialization;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector3InverseTransformDirection : StateFunctionFPVector3
    {
        public HNSFParamFPVector3 directionVector;
        [FormerlySerializedAs("localVector")] public HNSFParamFPVector3 worldVector;
        
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var fpQ = FPQuaternion.LookRotation(directionVector.Resolve(frame, entity, ref stateContext));
            var lv = worldVector.Resolve(frame, entity, ref stateContext);
            return lv.InverseTransformDirection(fpQ);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector3InverseTransformDirection());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector3InverseTransformDirection;
            t.directionVector = directionVector.Clone() as HNSFParamFPVector3;
            t.worldVector = worldVector.Clone() as HNSFParamFPVector3;
            return base.CopyTo(target);
        }
    }
}