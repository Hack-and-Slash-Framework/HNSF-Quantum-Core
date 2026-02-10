using HnSF.core.state;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ComboProration
    {
        public FP GetProratedDamage(FP damage, int comboCounter, FP globalComboProration)
        {
            if (bonusProration > 0) damage *= bonusProration;
            if (comboCounter > 0) damage *= globalComboProration;
            return damage * currentProration;
        }

        public void ApplyBonusProration(FP bonus)
        {
            if (bonus == 0 || bonusProration > 0) return;
            bonusProration = bonus;
        }

        public void AddAttackToHitList(Frame frame, EntityRef attackerRef, AssetRef<HNSFState> stateRef, uint stateId)
        {
            if (stateRef == default) return;
            var stateList = frame.ResolveList(hitByAttacks);

            for (int i = stateList.Count - 1; i >= 0; i--)
            {
                if (attackerRef != stateList[i].attackerRef || stateRef != stateList[i].stateRef ||
                    stateId != stateList[i].stateId)
                {
                    continue;
                }

                return;
            }

            stateList.Add(new UniqueComboAttackIdentifier()
            {
                attackerRef = attackerRef,
                stateRef = stateRef,
                stateId = stateId
            });
        }

        public bool AlreadyHitByExactAttack(Frame frame, EntityRef attackerRef, AssetRef<HNSFState> stateRef,
            uint stateId)
        {
            if (stateRef == default) return false;
            var stateList = frame.ResolveList(hitByAttacks);

            for (int i = stateList.Count - 1; i >= 0; i--)
            {
                if (attackerRef == stateList[i].attackerRef && stateRef == stateList[i].stateRef &&
                    stateId == stateList[i].stateId) return true;
            }

            return false;
        }

        public bool AttackAlreadyInList(Frame frame, AssetRef<HNSFState> stateRef)
        {
            if (stateRef == default) return false;
            var stateList = frame.ResolveList(hitByAttacks);
            
            for (int i = 0; i < stateList.Count; i++)
            {
                if (stateList[i].stateRef == stateRef) return true;
            }
            return false;
        }
        
        public int AttackInListXTimes(Frame frame, AssetRef<HNSFState> stateRef)
        {
            if (stateRef == default) return 0;
            var stateList = frame.ResolveList(hitByAttacks);

            int counter = 0;

            for (int i = 0; i < stateList.Count; i++)
            {
                if (stateList[i].stateRef == stateRef) counter++;
            }

            return counter;
        }

        public int AttackInListXTimes(Frame frame, EntityRef attackerRef, AssetRef<HNSFState> stateRef)
        {
            if (stateRef == default) return 0;
            var stateList = frame.ResolveList(hitByAttacks);

            int counter = 0;

            for (int i = 0; i < stateList.Count; i++)
            {
                if (stateList[i].stateRef == stateRef && stateList[i].attackerRef == attackerRef) counter++;
            }

            return counter;
        }
    }
}