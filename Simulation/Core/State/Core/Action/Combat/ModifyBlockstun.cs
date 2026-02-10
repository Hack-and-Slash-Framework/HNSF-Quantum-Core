using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyBlockstun : HNSFStateAction
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
            if (!frame.Unsafe.TryGetPointer<Blockstun>(entity, out var blockstun)) return false;

            switch (modifyType)
            {
                case ModifyType.SET:
                    blockstun->value = value;
                    break;
                case ModifyType.ADD:
                    blockstun->value += value;
                    break;
                case ModifyType.MULTIPLY:
                    blockstun->value *= value;
                    break;
            }

            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyBlockstun());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyBlockstun;
            t.modifyType = modifyType;
            t.value = value;
            return base.CopyTo(target);
        }
    }
}