using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ResetComboProration : HNSFStateAction
    {
        public bool resetProration = true;
        public bool resetHitByAttackList = true;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer(entity, out ComboProration* comboProration))
            {
                if (resetProration)
                {
                    comboProration->comboDecay = 0;
                    comboProration->currentProration = 1;
                    comboProration->bonusProration = 0;
                }
                if (resetHitByAttackList)
                {
                    var hitByAttacks = frame.ResolveList(comboProration->hitByAttacks);
                    hitByAttacks.Clear();
                }
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ResetComboProration());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ResetComboProration;
            t.resetProration = resetProration;
            t.resetHitByAttackList = resetHitByAttackList;
            return base.CopyTo(target);
        }
    }
}