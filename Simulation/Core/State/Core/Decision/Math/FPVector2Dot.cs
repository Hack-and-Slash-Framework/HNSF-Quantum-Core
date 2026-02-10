using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class FPVector2Dot : HNSFStateDecision
    {
        public HNSFParamFPVector2 inputAParam;
        public HNSFParamFPVector2 inputBParam;

        public HNSFParamFP minValueParam;
        public HNSFParamFP maxValueParam;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var inputA = inputAParam.Resolve(frame, entity, ref stateContext);
            var inputB = inputBParam.Resolve(frame, entity, ref stateContext);

            var dotResult = FPVector2.Dot(inputA, inputB);

            return dotResult >= minValueParam.Resolve(frame, entity, ref stateContext)
                   && dotResult <= maxValueParam.Resolve(frame, entity, ref stateContext);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new FPVector2Dot());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as FPVector2Dot;
            t.inputAParam = inputAParam.Clone() as HNSFParamFPVector2;
            t.inputBParam = inputBParam.Clone() as HNSFParamFPVector2;
            t.minValueParam = minValueParam.Clone() as HNSFParamFP;
            t.maxValueParam = maxValueParam.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}