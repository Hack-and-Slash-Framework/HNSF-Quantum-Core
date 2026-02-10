using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Modify Animation Frame")]
    public unsafe partial class ModifyAnimationFrame : HNSFStateAction
    {
        public enum ModifyType
        {
            ADD,
            SET
        }

        public int layer;
        public ModifyType modify;
        public int value;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var animator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);

            switch (modify)
            {
                case ModifyType.ADD:
                    animator->state.layers[layer].frame += value;
                    break;
                case ModifyType.SET:
                    animator->state.layers[layer].frame = value;
                    break;
            }

            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyAnimationFrame());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyAnimationFrame;
            t.layer = layer;
            t.modify = modify;
            t.value = value;
            return base.CopyTo(target);
        }
    }
}