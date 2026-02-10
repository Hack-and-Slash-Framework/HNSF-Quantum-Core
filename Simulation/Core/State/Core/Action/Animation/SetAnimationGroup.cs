using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation Group")]
    public unsafe partial class SetAnimationGroup : HNSFStateAction
    {
        public AssetRef<AnimationGroupDefinitions> animationGroupRef;
        
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
            animator->animationDefinitionsReference = animationGroupRef;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimationGroup());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimationGroup;
            t.animationGroupRef = animationGroupRef;
            return base.CopyTo(target);
        }
    }
}