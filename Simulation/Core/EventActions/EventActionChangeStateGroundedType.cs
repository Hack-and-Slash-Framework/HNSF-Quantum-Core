using Quantum;

namespace HnSF
{
    public unsafe class EventActionChangeStateGroundedType : HNSFEventAction
    {
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)) return;
            if (!frame.TryFindAsset(gsm->stateAgent.stateData.toState, out var toState)) return;
            if (toState.initialGroundedState == StateGroundedType.NONE) return;
            if (!frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var bap)) return;

            if (toState.initialGroundedState == StateGroundedType.GROUNDED
                && bap->currentGroundedState == StateGroundedType.AERIAL)
            {
                if (gsm->blackboard.Board.IsValid)
                {
                    gsm->blackboard.Set(frame, "CurrAirJump", 0);
                    gsm->blackboard.Set(frame, "CurrAirDash", 0);
                }
            }
            
            bool changedState = toState.initialGroundedState != bap->currentGroundedState;
            bap->currentGroundedState = toState.initialGroundedState;
            if (changedState)
            {
                EventReceiverHelper.CallEvent(frame, entity, (int)EventReceiverTyping.GroundedStateChanged);
            }
        }
    }
}