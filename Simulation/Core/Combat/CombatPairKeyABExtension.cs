using System;

namespace Quantum
{
    public unsafe partial struct CombatPairKeyAB : IEquatable<CombatPairKeyAB>
    {
        public bool Equals(CombatPairKeyAB other)
        {
            return entityA.Equals(other.entityA) && entityB.Equals(other.entityB);
        }

        public override bool Equals(object obj)
        {
            return obj is CombatPairKeyAB other && Equals(other);
        }
    }
}