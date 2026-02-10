using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class FPVector3Dot : HNSFStateDecision
    {
        public HNSFParamFPVector3 inputAParam;
        public HNSFParamFPVector3 inputBParam;

        public HNSFParamFP minValueParam;
        public HNSFParamFP maxValueParam;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var inputA = inputAParam.Resolve(frame, entity, ref stateContext);
            var inputB = inputBParam.Resolve(frame, entity, ref stateContext);

            var dotResult = FPVector3.Dot(inputA, inputB);

            return dotResult >= minValueParam.Resolve(frame, entity, ref stateContext)
                   && dotResult <= maxValueParam.Resolve(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new FPVector3Dot());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as FPVector3Dot;
            t.inputAParam = inputAParam.Clone() as HNSFParamFPVector3;
            t.inputBParam = inputBParam.Clone() as HNSFParamFPVector3;
            t.minValueParam = minValueParam.Clone() as HNSFParamFP;
            t.maxValueParam = maxValueParam.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}