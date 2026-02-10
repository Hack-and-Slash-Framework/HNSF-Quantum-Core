using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation")]
    public unsafe partial class SetAnimation : HNSFStateAction
    {
        public int layer;
        public AssetRef<AnimationEntry> animToPlay;
        public AssetRef<Tag> layerMask;
        public bool setFrame = true;
        [DrawIf(nameof(setFrame), true)]
        public int startFrame = 0;
        public bool ignoreIfAlreadyPlaying;
        public bool autoPlay;
        public int autoPlayAdvanceBy = 0;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity)
        {
            var animator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);
            if (ignoreIfAlreadyPlaying && animator->state.layers[layer].animationEntry.Id == animToPlay.Id) return;
            animator->state.layers[layer].animationEntry = animToPlay;
            animator->state.layers[layer].weight = 1;
            if (setFrame) animator->state.layers[layer].frame = startFrame;
            animator->state.layers[layer].autoPlay = autoPlay;
            animator->state.layers[layer].autoPlayAdvanceAmount = autoPlayAdvanceBy;
            animator->state.layers[layer].mask = layerMask;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimation());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimation;
            t.layer = layer;
            t.animToPlay = animToPlay;
            t.setFrame = setFrame;
            t.startFrame = startFrame;
            t.ignoreIfAlreadyPlaying = ignoreIfAlreadyPlaying;
            t.autoPlay = autoPlay;
            t.autoPlayAdvanceBy = autoPlayAdvanceBy;
            t.layerMask = layerMask;
            return base.CopyTo(target);
        }
    }
}