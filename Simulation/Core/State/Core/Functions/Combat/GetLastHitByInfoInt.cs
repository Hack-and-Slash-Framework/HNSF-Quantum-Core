using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetLastHitByInfoInt : StateFunctionInt
    {
        public enum HitInfoValueType
        {
            ClashLevel,
            Damage,
            ChipDamage,
            Hitstun,
            Hitstop,
            Untech,
            Blockstun
        }

        public HitInfoValueType hitInfoValue;
    
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitByInfo>(entity, out var boxCombatant)
                || !frame.TryFindAsset<HitInfo>(boxCombatant->hitByInfo.Id, out var hitInfo)) return 0;

            switch (hitInfoValue)
            {
                case HitInfoValueType.ClashLevel:
                    return (int)hitInfo.clashLevel;
                case HitInfoValueType.Damage:
                    return (int)hitInfo.damage;
                case HitInfoValueType.ChipDamage:
                    return (int)hitInfo.chipDamage;
                case HitInfoValueType.Hitstun:
                    return (int)hitInfo.hitstun;
                case HitInfoValueType.Hitstop:
                    return (int)hitInfo.hitstop;
                case HitInfoValueType.Untech:
                    return (int)hitInfo.untech;
                case HitInfoValueType.Blockstun:
                    return (int)hitInfo.blockstun;
                default:
                    return 0;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetLastHitByInfoInt());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetLastHitByInfoInt;
            t.hitInfoValue = hitInfoValue;
            return base.CopyTo(target);
        }
    }
}