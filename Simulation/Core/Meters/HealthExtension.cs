using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Health
    {
        public void ApplyDamage(Frame frame, EntityRef entity, int damage, bool doesNotKill, bool clampAtZero = true)
        {
            var oldHealth = value;
            ApplyDamage_NoSignal(damage, doesNotKill, clampAtZero);
            if (oldHealth == value) return;
            frame.Signals.HealthDecreased(entity, oldHealth, value);
        }
        
        public void ApplyDamage_NoSignal(int damage, bool doesNotKill, bool clampAtZero = true)
        {
            if (damage <= 1) damage = 1;
            if (doesNotKill && damage >= value)
            {
                value = 1;
                return;
            }

            var oldHealth = value;
            value -= damage;
            if (clampAtZero && value < 0) value = 0;
        }

        public void ApplyHealing(Frame frame, EntityRef entity, int healing, int maxHealth)
        {
            var oldHealth = value;
            ApplyHealing_NoSignal(healing, maxHealth);
            if (oldHealth == value) return;
            frame.Signals.HealthIncreased(entity, oldHealth, value);
        }
        
        public void ApplyHealing_NoSignal(int healing, int maxHealth)
        {
            value = FPMath.Clamp(value+healing, 0, maxHealth);
        }
    }
}