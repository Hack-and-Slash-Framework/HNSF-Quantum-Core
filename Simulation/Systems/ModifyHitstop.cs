using Quantum;

namespace HnSF.core.systems
{
    public unsafe class ModifyHitstop : SystemMainThreadFilter<ModifyHitstop.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Hitstop* hitstop;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.hitstop->value <= 0) return;
            filter.hitstop->value -= 1;
        }
    }
}