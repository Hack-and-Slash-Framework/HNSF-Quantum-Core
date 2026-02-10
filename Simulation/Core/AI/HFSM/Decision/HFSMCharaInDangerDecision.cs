using Quantum;

namespace HnSF.core.state.decisions
{
    [System.Serializable]
    public unsafe partial class HFSMCharaInDangerDecision : HFSMDecision
    {
        public AIBlackboardValueKey attackOriginPoint;
        public AIBlackboardValueKey attackOriginEntity;
        
        public override bool Decide(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!CombatHelper.WillBeHitThisFrame(frame, entity, out EntityRef originEntityRef, returnHitboxIfNoOwner: true, failIfNoEntity: false)) return false;

            if (!string.IsNullOrEmpty(attackOriginPoint.Key) && frame.Unsafe.TryGetPointer<Transform3D>(originEntityRef, out var originTransform))
            {
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, attackOriginPoint.Key, originTransform->Position);
            }

            if (!string.IsNullOrEmpty(attackOriginEntity.Key))
            {
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, attackOriginEntity.Key, originEntityRef);
            }
            return true;
        }
    }
}