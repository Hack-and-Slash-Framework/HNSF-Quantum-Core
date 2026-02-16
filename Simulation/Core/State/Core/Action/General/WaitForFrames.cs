using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class WaitForFrames : HNSFStateAction
    {
        public int framesToWait = 60;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            if (!IsTimerOver(frame, targetEntityRef)) stateContext.agentData->dontAutoIncrementFrame = true;
            frame.Remove<GenericTimer>(targetEntityRef);
            return false;
        }

        private bool IsTimerOver(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<GenericTimer>(entityRef, out var timer))
            {
                frame.Add(entityRef, new GenericTimer()
                {
                    countingType = TimerCountingType.CountDown,
                    value = framesToWait
                });
                return false;
            }

            return timer->value <= 0;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new WaitForFrames());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as WaitForFrames;
            t.framesToWait = framesToWait;
            return base.CopyTo(target);
        }
    }
}