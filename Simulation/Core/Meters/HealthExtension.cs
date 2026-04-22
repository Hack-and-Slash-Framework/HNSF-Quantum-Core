using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Health
    {
        public int ApplyDamage(Frame frame, EntityRef entity, int damage, bool doesNotKill, bool clampAtZero = true)
        {
            var healthChangeResult = new HealthChangeResult()
            {
                Ignore = false,
                From = value,
                To = value - GetExpectedDamage(damage, doesNotKill, clampAtZero)
            };
            frame.Signals.PreHealthChange(entity, &healthChangeResult);
            if (healthChangeResult.Ignore || healthChangeResult.From == healthChangeResult.To) return 0;
            return ApplyDamage_NoSignal((healthChangeResult.From - healthChangeResult.To), doesNotKill, clampAtZero);
        }

        private int GetExpectedDamage(int damage, bool doesNotKill, bool clampAtZero = true)
        {
            if (damage >= value)
            {
                if (doesNotKill) return value - 1;
                if (clampAtZero) return value;
            }
            return damage;
        }
        
        public int ApplyDamage_NoSignal(int damage, bool doesNotKill, bool clampAtZero = true)
        {
            damage = GetExpectedDamage(damage, doesNotKill, clampAtZero);
            value -= damage;
            return damage;
        }

        public void ApplyHealing(Frame frame, EntityRef entity, int healing, int maxHealth)
        {
            var healthChangeResult = new HealthChangeResult()
            {
                Ignore = false,
                From = value,
                To = value + healing
            };
            frame.Signals.PreHealthChange(entity, &healthChangeResult);
            if (healthChangeResult.Ignore || healthChangeResult.From == healthChangeResult.To) return;
            ApplyHealing_NoSignal((healthChangeResult.To - healthChangeResult.From), maxHealth);
        }
        
        public void ApplyHealing_NoSignal(int healing, int maxHealth)
        {
            value = FPMath.Clamp(value+healing, 0, maxHealth);
        }
    }
}