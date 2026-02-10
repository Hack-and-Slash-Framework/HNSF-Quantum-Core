using Quantum;

namespace HnSF.core.systems
{
    public unsafe class GenericTimerTick : SystemMainThreadFilter<GenericTimerTick.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GenericTimer* GenericTimer;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            switch (filter.GenericTimer->countingType)
            {
                case TimerCountingType.CountDown:
                    if (filter.GenericTimer->value > 0) filter.GenericTimer->value -= 1;
                    break;
                case TimerCountingType.CountUp:
                    if (filter.GenericTimer->value < int.MaxValue) filter.GenericTimer->value += 1;
                    break;
            }
        }
    }
}