using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetCombatConfigInt : StateFunctionInt
    {
        public enum CombatConfigValueType
        {
            MaxWallBounces,
            MaxGroundBounces,
            MaxHardKnockdowns
        }

        public CombatConfigValueType combatConfigValue;
    
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(frame.RuntimeConfig.combatConfigAssetRef, out var combatConfig)) return 0;

            switch (combatConfigValue)
            {
                case CombatConfigValueType.MaxWallBounces:
                    return combatConfig.maxWallBounces;
                case CombatConfigValueType.MaxGroundBounces:
                    return combatConfig.maxGroundBounces;
                case CombatConfigValueType.MaxHardKnockdowns:
                    return combatConfig.maxHardKnockdowns;
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
            var t = target as GetCombatConfigInt;
            t.combatConfigValue = combatConfigValue;
            return base.CopyTo(target);
        }
    }
}