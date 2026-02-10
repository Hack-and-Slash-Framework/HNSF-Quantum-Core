using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class FPVector3SignedAngle : HNSFStateDecision
    {
        public HNSFParamFPVector3 inputAParam;
        public HNSFParamFPVector3 inputBParam;
        public HNSFParamFPVector3 axisParam;

        public HNSFParamFP minAngleParam;
        public HNSFParamFP maxAngleParam;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var inputA = inputAParam.Resolve(frame, entity, ref stateContext);
            var inputB = inputBParam.Resolve(frame, entity, ref stateContext);
            var axis = axisParam.Resolve(frame, entity, ref stateContext);

            var angleResult = FPVector3.SignedAngle(inputA, inputB, axis);

            return angleResult >= minAngleParam.Resolve(frame, entity, ref stateContext)
                   && angleResult <= maxAngleParam.Resolve(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new FPVector3SignedAngle());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as FPVector3SignedAngle;
            t.inputAParam = inputAParam.Clone() as HNSFParamFPVector3;
            t.inputBParam = inputBParam.Clone() as HNSFParamFPVector3;
            t.axisParam = axisParam.Clone() as HNSFParamFPVector3;
            t.minAngleParam = minAngleParam.Clone() as HNSFParamFP;
            t.maxAngleParam = maxAngleParam.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}