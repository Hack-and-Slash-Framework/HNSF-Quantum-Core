using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation From Tag")]
    public unsafe partial class SetAnimationFromTag : HNSFStateAction
    {
        public int layer;
        public AssetRef<Tag> animTag;
        public AssetRef<Tag> layerMask;
        public bool ignoreIfAlreadyPlaying;
        public bool autoPlay;
        public int throweeId;
        public int startFrame = 0;
        
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
            
            AssetRef<AnimationEntry> anim = default;
            
            if (frame.TryFindAsset(animator->animationDefinitionsReference, out var animationGroupings))
                animationGroupings.TryGetAnimationByTag(frame, animTag, out anim);

            if (ignoreIfAlreadyPlaying && animator->state.layers[layer].animationEntry.Id == anim.Id) return;
            animator->state.layers[layer].animationEntry = anim;
            animator->state.layers[layer].weight = 1;
            animator->state.layers[layer].frame = startFrame;
            animator->state.layers[layer].autoPlay = autoPlay;
            animator->state.layers[layer].mask = layerMask;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimationFromTag());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimationFromTag;
            t.layer = layer;
            t.animTag = animTag;
            t.ignoreIfAlreadyPlaying = ignoreIfAlreadyPlaying;
            t.autoPlay = autoPlay;
            t.throweeId = throweeId;
            t.startFrame = startFrame;
            t.layerMask = layerMask;
            return base.CopyTo(target);
        }
    }
}