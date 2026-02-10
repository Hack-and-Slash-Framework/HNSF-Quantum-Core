using Quantum;

namespace HnSF
{
    public unsafe class EventActionExitedAerialGroundedType : HNSFEventAction
    {
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var physics)) return;
            if (physics->currentGroundedState == StateGroundedType.AERIAL) return;
        }
    }
}