using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyHitstun : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            ADD,
            MULTIPLY
        }

        public enum ScalingType
        {
            Raw,
            Hitstun,
            Untech
        }

        public ModifyType modifyType;
        public ScalingType scalingType;
        public HNSFParamInt valueParam;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Hitstun>(entity, out var hitstun)) return false;

            var value = valueParam.Resolve(frame, entity, ref stateContext);
            
            switch (modifyType)
            {
                case ModifyType.SET:
                    hitstun->value = value;
                    break;
                case ModifyType.ADD:
                    hitstun->value += value;
                    break;
                case ModifyType.MULTIPLY:
                    hitstun->value *= value;
                    break;
            }

            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyHitstun());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyHitstun;
            t.modifyType = modifyType;
            t.scalingType = scalingType;
            t.valueParam = valueParam.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}