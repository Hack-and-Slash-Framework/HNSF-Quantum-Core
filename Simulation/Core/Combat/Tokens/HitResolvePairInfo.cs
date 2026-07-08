namespace Quantum
{
    public unsafe partial struct HitResolvePairInfo
    {
        public Frame Frame;
        public EntityRef AttackerEntityRef;
        public EntityRef DefenderEntityRef;
        public DefenderHitResultData DefenderHitResultData;
        public Hitbox* attackerHitbox;
        public Hurtbox* defenderHurtbox;
    }
}