using System;
using HnSF.core;

namespace Quantum
{
    [Serializable]
    public unsafe partial class HFSMFindDangerSourceAction : AIAction
    {
        public AIBlackboardValueKey attackOriginPoint;
        public AIBlackboardValueKey attackOriginEntity;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (!CombatHelper.WillBeHitThisFrame(frame, entity, out EntityRef originEntityRef, returnHitboxIfNoOwner: true, failIfNoEntity: false)) return;

            if (!string.IsNullOrEmpty(attackOriginPoint.Key) && frame.Unsafe.TryGetPointer<Transform3D>(originEntityRef, out var originTransform))
            {
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, attackOriginPoint.Key, originTransform->Position);
            }

            if (!string.IsNullOrEmpty(attackOriginEntity.Key))
            {
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, attackOriginEntity.Key, EntityRef.None);
            }
        }
    }
}