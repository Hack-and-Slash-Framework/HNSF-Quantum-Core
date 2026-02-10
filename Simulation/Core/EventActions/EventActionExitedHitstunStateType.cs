using Quantum;

namespace HnSF
{
    public unsafe class EventActionExitedHitstunStateType : HNSFEventAction
    {
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)) return;
            if (!frame.TryFindAsset(gsm->stateAgent.stateData.state, out var currentState) ||
                !frame.TryFindAsset(gsm->stateAgent.stateData.toState, out var toState)) return;
            if (currentState.stateType != frame.SimulationConfig.stateType_Hitstun) return;
            if (currentState.stateType == toState.stateType) return;

            if (frame.Unsafe.TryGetPointer(entity, out ComboCounting* comboCounting))
            {
                comboCounting->comboCounter = 0;
                comboCounting->realComboCounter = 0;
            }

            if (frame.Unsafe.TryGetPointer(entity, out ComboProration* comboProration))
            {
                comboProration->comboDecay = 0;
                comboProration->currentProration = 1;
                comboProration->bonusProration = 0;
                var hitByAttacks = frame.ResolveList(comboProration->hitByAttacks);
                hitByAttacks.Clear();
            }
            
            gsm->blackboard.Set(frame, "ShouldGroundBounce", false);
            gsm->blackboard.Set(frame, "CurrGroundBounce", 0);
            gsm->blackboard.Set(frame, "ShouldHardKnockdown", false);
            gsm->blackboard.Set(frame, "CurrHardKnockdown", 0);
            gsm->blackboard.Set(frame, "ShouldWallBounce", false);
            gsm->blackboard.Set(frame, "CurrWallBounce", 0);

            if (frame.Unsafe.TryGetPointer(entity, out BattleActorPhysics* physicsActor)
                && frame.TryFindAsset(gsm->config, out var aiConfig))
            {
                physicsActor->pushStrength = aiConfig.Get("DefaultPushStrength").Value.FP;
                physicsActor->selfPushStrength = aiConfig.Get("DefaultSelfPushStrength").Value.FP;
            }

            if (frame.Unsafe.TryGetPointer(entity, out Hitstun* hitstun))
            {
                hitstun->value = 0;
            }
        }
    }
}