using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetLastHitByInfoBool : StateFunctionBool
    {
        public enum HitInfoValueType
        {
            
        }

        public HitInfoValueType hitInfoValue;
    
        public override bool Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<LastHitByInfo>(entity, out var boxCombatant)
                || !frame.TryFindAsset<HitInfo>(boxCombatant->hitByInfo.Id, out var hitInfo)) return false;

            switch (hitInfoValue)
            {
                default:
                    return false;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetLastHitByInfoBool());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetLastHitByInfoBool;
            t.hitInfoValue = hitInfoValue;
            return base.CopyTo(target);
        }
    }
}