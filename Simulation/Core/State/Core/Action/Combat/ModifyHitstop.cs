using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyHitstop : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            ADD,
            MULTIPLY
        }

        public ModifyType modifyType;
        public int value;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Hitstop>(entity, out var hitstop)) return false;

            switch (modifyType)
            {
                case ModifyType.SET:
                    hitstop->value = value;
                    break;
                case ModifyType.ADD:
                    hitstop->value += value;
                    break;
                case ModifyType.MULTIPLY:
                    hitstop->value *= value;
                    break;
            }

            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyHitstop());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyHitstop;
            t.modifyType = modifyType;
            t.value = value;
            return base.CopyTo(target);
        }
    }
}