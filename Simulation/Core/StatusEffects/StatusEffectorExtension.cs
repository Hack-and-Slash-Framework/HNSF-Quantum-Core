using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct StatusEffector
    {
        public void RemoveStacks(int stackCount, bool allStacks = false)
        {
            if (allStacks) stackCount = stacks;
            stacks = FPMath.Clamp(stacks - stackCount, 0, int.MaxValue);
        }

        public void RemoveStatusEffect(Frame frame, EntityRef statusEffectorEntityRef)
        {
            if (!frame.Unsafe.TryGetPointer<StatusEffectActor>(actor, out var statusEffectActor))
            {
                frame.Destroy(statusEffectorEntityRef);
                return;
            }

            statusEffectActor->RemoveStatusEffect(frame, actor, statusEffectorEntityRef);
        }
    }
}