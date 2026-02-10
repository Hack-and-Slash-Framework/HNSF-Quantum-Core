using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetLastHitByInfoFP : StateFunctionFP
    {
        public enum HitInfoValueType
        {
            
        }

        public HitInfoValueType hitInfoValue;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitByInfo>(entity, out var boxCombatant)
                || !frame.TryFindAsset<HitInfo>(boxCombatant->hitByInfo.Id, out var hitInfo)) return 0;

            switch (hitInfoValue)
            {
                default:
                    return 0;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetLastHitByInfoFP());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetLastHitByInfoFP;
            t.hitInfoValue = hitInfoValue;
            return base.CopyTo(target);
        }
    }
}