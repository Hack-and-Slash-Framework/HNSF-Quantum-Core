using Quantum;

namespace HnSF.core.systems
{
    public unsafe class DecrementHitstun : SystemMainThreadFilter<DecrementHitstun.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Hitstun* Hitstun;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.Hitstun->value <= 0) return;
            if (f.Unsafe.TryGetPointer(filter.Entity, out Hitstop* hitstop) && hitstop->value > 0) return;
            filter.Hitstun->value -= 1;
        }
    }
}