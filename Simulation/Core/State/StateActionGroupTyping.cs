using System;

namespace HnSF.core.state
{
    [Serializable]
    [Flags]
    public enum StateActionGroupTyping
    {
        None = 0 ,
        Hitbox = 1 << 0,
        Hurtbox = 1 << 1,
        Pushbox = 1 << 2,
        Throwbox = 1 << 3,
        Collisionbox = 1 << 4,
        Warningbox = 1 << 5,
        StateTransition = 1 << 6,
        StatusEffects = 1 << 7,
        Animation = 1 << 8,
        Debug = 1 << 9,
    }
}
