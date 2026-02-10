using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetLastHitByInfoFPVector3 : StateFunctionFPVector3
    {
        public enum HitInfoValueType
        {
            
        }

        public HitInfoValueType hitInfoValue;
    
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitByInfo>(entity, out var boxCombatant)
                || !frame.TryFindAsset<HitInfo>(boxCombatant->hitByInfo.Id, out var hitInfo)) return FPVector3.Zero;

            switch (hitInfoValue)
            {
                default:
                    return FPVector3.Zero;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetLastHitByInfoFPVector3());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetLastHitByInfoFPVector3;
            t.hitInfoValue = hitInfoValue;
            return base.CopyTo(target);
        }
    }
}