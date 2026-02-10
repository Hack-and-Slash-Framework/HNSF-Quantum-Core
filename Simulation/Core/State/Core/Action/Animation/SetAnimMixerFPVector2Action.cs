using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation Mixer FPVector2")]
    public unsafe partial class SetAnimationMixerFPVector2 : HNSFStateAction
    {
        public int layer;
        public HNSFParamFPVector2 param = new HNSFParamFPVector2();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var charaAnimator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);
            charaAnimator->state.layers[layer].mixerParam = param.Resolve(frame, entity, ref stateContext);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimationMixerFPVector2());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimationMixerFPVector2;
            t.layer = layer;
            t.param = param.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}