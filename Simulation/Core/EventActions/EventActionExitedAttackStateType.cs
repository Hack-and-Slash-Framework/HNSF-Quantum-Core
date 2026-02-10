using Quantum;

namespace HnSF
{
    public unsafe class EventActionExitedAttackStateType : HNSFEventAction
    {
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)) return;
            if (!frame.TryFindAsset(gsm->stateAgent.stateData.state, out var currentState) ||
                !frame.TryFindAsset(gsm->stateAgent.stateData.toState, out var toState)) return;
            if (currentState.stateType != frame.SimulationConfig.stateType_Attack) return;
            if (currentState.stateType == toState.stateType) return;

            frame.Remove<TrackingAttackString>(entity);
        }
    }
}
