using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Modify Animator Autoplay Amount")]
    public unsafe partial class ModifyActorAnimatorAutoplayAmount : HNSFStateAction
    {
        public int layer = 0;
        public int autoPlayAdvanceBy = 0;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var animator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);
            animator->state.layers[layer].autoPlayAdvanceAmount = autoPlayAdvanceBy;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyActorAnimatorAutoplayAmount());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyActorAnimatorAutoplayAmount;
            t.layer = layer;
            t.autoPlayAdvanceBy = autoPlayAdvanceBy;
            return base.CopyTo(target);
        }
    }
}