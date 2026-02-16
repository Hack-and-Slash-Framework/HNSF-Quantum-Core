using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DoCharge : HNSFStateAction
    {
        public int[] chargePerLevel = new int[1];
        public bool canHoldCharge = false;
        public ActorInputButtonType buttonsToHold;
        public bool holdOnLastRangeFrame = false;

        [NonSerialized] private int _maxChargeAmount = -1;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ActorInputBuffer>(entity, out var charaInputs)) return false;

            _maxChargeAmount = GetMaxCharge();
            
            frame.AddOrGet<IsChargingAttack>(entity, out var isCharging);
            isCharging->newChargeLevel = false;
            int currentChargeLevel = isCharging->chargeLevel;
            isCharging->maxCharge = _maxChargeAmount;

            var bData = InputHelper.GetButtonData(frame, charaInputs, buttonsToHold,
                startOffset: 0,
                bufferFrames: 0,
                checkType: ButtonDataCheckType.ALL,
                ignoreDisabledInputs: false);
            
            if (bData.IsDown == false || (canHoldCharge == false && isCharging->currentCharge >= _maxChargeAmount))
            {
                return false;
            }

            if(isCharging->currentCharge < _maxChargeAmount) isCharging->currentCharge++;
            isCharging->chargeLevel = GetChargeLevel(isCharging->currentCharge);
            stateContext.agentData->dontAutoIncrementFrame = true;
            if (isCharging->chargeLevel != currentChargeLevel) isCharging->newChargeLevel = true;
            return false;
        }
        
        private int GetChargeLevel(int currentCharge)
        {
            int s = 0;
            for (int i = 0; i < chargePerLevel.Length; i++)
            {
                s += chargePerLevel[i];
                if (currentCharge < s) return i;
            }
            return chargePerLevel.Length;
        }

        private int GetMaxCharge()
        {
            return chargePerLevel.Sum();
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DoCharge());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DoCharge;
            t.chargePerLevel = chargePerLevel.ToArray();
            t.canHoldCharge = canHoldCharge;
            t.buttonsToHold = buttonsToHold;
            t.holdOnLastRangeFrame = holdOnLastRangeFrame;
            return base.CopyTo(target);
        }
    }
}