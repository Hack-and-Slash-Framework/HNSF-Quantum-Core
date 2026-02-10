using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class FPVector2SignedAngle : HNSFStateDecision
    {
        public HNSFParamFPVector2 inputAParam;
        public HNSFParamFPVector2 inputBParam;

        public HNSFParamFP minAngleParam;
        public HNSFParamFP maxAngleParam;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var inputA = inputAParam.Resolve(frame, entity, ref stateContext);
            var inputB = inputBParam.Resolve(frame, entity, ref stateContext);

            var angleResult = FPVector2.RadiansSigned(inputA, inputB) * FP.Rad2Deg;

            return angleResult >= minAngleParam.Resolve(frame, entity, ref stateContext)
                   && angleResult <= maxAngleParam.Resolve(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new FPVector2SignedAngle());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as FPVector2SignedAngle;
            t.inputAParam = inputAParam.Clone() as HNSFParamFPVector2;
            t.inputBParam = inputBParam.Clone() as HNSFParamFPVector2;
            t.minAngleParam = minAngleParam.Clone() as HNSFParamFP;
            t.maxAngleParam = maxAngleParam.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}