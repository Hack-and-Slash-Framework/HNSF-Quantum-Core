using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetLastHitByInfoFPVector2 : StateFunctionFPVector2
    {
        public enum HitInfoValueType
        {
            
        }

        public HitInfoValueType hitInfoValue;
    
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitByInfo>(entity, out var boxCombatant)
                || !frame.TryFindAsset<HitInfo>(boxCombatant->hitByInfo.Id, out var hitInfo)) return FPVector2.Zero;

            switch (hitInfoValue)
            {
                default:
                    return FPVector2.Zero;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetLastHitByInfoFPVector2());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetLastHitByInfoFPVector2;
            t.hitInfoValue = hitInfoValue;
            return base.CopyTo(target);
        }
    }
}