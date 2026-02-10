using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial class FindEnemyAction : AIAction
    {
        public bool emptySlot = true;
        public AIBlackboardValueKey targetBlackboardKey;
        public FP checkRadius;
        public LayerMask layerMask;
        public int runEvery = 0;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            if (runEvery > 0 && frame.Number % runEvery != 0) return;
            if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transformSelf)) return;
            
            if (emptySlot)
            {
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, targetBlackboardKey.Key, EntityRef.None);
            }
            
            var hr = frame.Physics3D.OverlapShape(transformSelf->Position, transformSelf->Rotation, Shape3D.CreateSphere(checkRadius), layerMask, QueryOptions.HitAll);
            if (hr.Count <= 0) return;

            for (int i = 0; i < hr.Count; i++)
            {
                if (hr[i].Entity == entity) continue;
                
                ((AIContextUser*)aiContext.UserData)->Blackboard->Set(frame, targetBlackboardKey.Key, hr[i].Entity);
                return;
            }
        }
    }
}
