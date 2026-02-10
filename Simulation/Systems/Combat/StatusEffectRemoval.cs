using Quantum;

namespace HnSF.core.systems
{
    public unsafe class StatusEffectRemoval : SystemMainThreadFilter<StatusEffectRemoval.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public StatusEffector* StatusEffector;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.StatusEffector->stacks > 0) return;

            filter.StatusEffector->RemoveStatusEffect(f, filter.Entity);
        }
    }
}