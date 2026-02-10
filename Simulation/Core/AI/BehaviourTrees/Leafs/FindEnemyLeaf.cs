using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe class FindEnemyLeaf : BTLeaf
    {
        public bool emptySlot = true;
        public AIBlackboardValueKey targetBlackboardKey;
        public bool globalCheck;
        [DrawIf(nameof(globalCheck), false)]
        public FP checkRadius;
        public LayerMask layerMask;
        
        public override void Init(BTParams btParams, ref AIContext aiContext)
        {
            base.Init(btParams, ref aiContext);
        }

        protected override BTStatus OnUpdate(BTParams btParams, ref AIContext aiContext)
        {
            if (globalCheck)
            {
                return CheckForTargetGlobally(btParams, ref aiContext);
            }
            else
            {
                return CheckForTargetWithinRadius(btParams);
            }
        }

        private BTStatus CheckForTargetGlobally(BTParams btParams, ref AIContext aiContext)
        {
            var frame = btParams.Frame as Frame;
            
            TeamBitmask hostilityMask = 0;
            
            if (frame.Unsafe.TryGetPointer<CombatTeam>(btParams.Entity, out var selfCombatTeam))
            {
                hostilityMask = selfCombatTeam->GetHostilityMask(frame);
            }
            
            EntityRef finalTargetEntityRef = default;
            
            var f = frame.Filter<Transform3D, BattleActorLink>();

            while (f.NextUnsafe(out var entityRef, out var targetTransform3D, out var targetFighter))
            {
                if (entityRef == btParams.Entity) continue;
                
                if(hostilityMask != 0 && frame.Unsafe.TryGetPointer<CombatTeam>(entityRef, out var targetCombatTeam))
                {
                    if (!CombatTeam.IsHostileTowards(frame, hostilityMask, targetCombatTeam->value)) continue;
                    finalTargetEntityRef = entityRef;
                    break;
                }
                else
                {
                    finalTargetEntityRef = entityRef;
                    break;
                }
            }
            
            if(finalTargetEntityRef == EntityRef.None) return BTStatus.Failure;
            btParams.Blackboard->Set(frame, targetBlackboardKey.Key, finalTargetEntityRef);
            return BTStatus.Success;
        }

        private BTStatus CheckForTargetWithinRadius(BTParams btParams)
        {
            var frame = btParams.Frame as Frame;
            
            var transformSelf = frame.Unsafe.GetPointer<Transform3D>(btParams.Entity);
            
            TeamBitmask hostilityMask = 0;
            if (frame.Unsafe.TryGetPointer<CombatTeam>(btParams.Entity, out var selfCombatTeam))
            {
                hostilityMask = selfCombatTeam->GetHostilityMask(frame);
            }
            
            if (emptySlot)
            {
                btParams.Blackboard->Set(frame, targetBlackboardKey.Key, EntityRef.None);
            }
            
            var hr = frame.Physics3D.OverlapShape(transformSelf->Position, transformSelf->Rotation, Shape3D.CreateSphere(checkRadius), layerMask, QueryOptions.HitAll);
            if(hr.Count <= 0) return BTStatus.Failure;

            EntityRef finalTargetEntityRef = default;
            
            for (int i = 0; i < hr.Count; i++)
            {
                if (hr[i].Entity == btParams.Entity) continue;
                
                if(hostilityMask != 0 && frame.Unsafe.TryGetPointer<CombatTeam>(hr[i].Entity, out var targetCombatTeam))
                {
                    if (!CombatTeam.IsHostileTowards(frame, hostilityMask, targetCombatTeam->value)) continue;
                    finalTargetEntityRef = hr[i].Entity;
                    break;
                }
                else
                {
                    finalTargetEntityRef = hr[i].Entity;
                    break;
                }
            }

            if(finalTargetEntityRef == EntityRef.None) return BTStatus.Failure;
            btParams.Blackboard->Set(frame, targetBlackboardKey.Key, finalTargetEntityRef);
            return BTStatus.Success;
        }
    }
}