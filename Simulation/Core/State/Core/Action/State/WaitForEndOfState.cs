using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class WaitForEndOfState : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            if (!IsLastFrameOfState(frame, targetEntityRef)) stateContext.agentData->dontAutoIncrementFrame = true;
            return false;
        }

        private bool IsLastFrameOfState(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var gsm)
                && frame.TryFindAsset(gsm->stateAgent.stateData.state, out var state))
            {
                return gsm->stateAgent.stateData.frame >= state.totalFrames;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new WaitForEndOfState());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as WaitForEndOfState;
            return base.CopyTo(target);
        }
    }
}