using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation Mixer FP")]
    public unsafe partial class SetAnimMixerFP : HNSFStateAction
    {
        public int layer;
        public HNSFParamFP param = new HNSFParamFP();

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var charaAnimator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);
            charaAnimator->state.layers[layer].mixerParam = new FPVector2(param.Resolve(frame, entity, ref stateContext), 0);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimMixerFP());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimMixerFP;
            t.layer = layer;
            t.param = param.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}