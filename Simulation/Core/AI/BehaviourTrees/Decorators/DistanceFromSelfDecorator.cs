using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe class DistanceFromSelfDecorator : BTDecorator
    {
        [System.Serializable]
        public enum CheckType
        {
            MORE_THAN,
            MORE_THAN_OR_EQUAL,
            LESS_THAN,
            LESS_THAN_OR_EQUAL,
        }
        
        public CheckType checkType;
        public FP comparingDistance;
        public AIBlackboardValueKey blackboardEntityRefKey;
        public bool invert;

        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            if (!btParams.Blackboard->TryGetEntityRef(btParams.Frame, blackboardEntityRefKey.Key, out var entityRef)
                || !btParams.Frame.Exists(entityRef)
                || !btParams.Frame.Unsafe.TryGetPointer<Transform3D>(entityRef, out var targetTransform3D)
                || !btParams.Frame.Unsafe.TryGetPointer<Transform3D>(btParams.Entity, out var selfTransform3D)) return false;

            var squaredDistance = FPVector3.DistanceSquared(selfTransform3D->Position, targetTransform3D->Position);
            var cDist = comparingDistance;
            var result = false;
            
            switch (checkType)
            {
                case CheckType.MORE_THAN:
                    result = squaredDistance > cDist;
                    break;
                case CheckType.MORE_THAN_OR_EQUAL:
                    result = squaredDistance >= cDist;
                    break;
                case CheckType.LESS_THAN:
                    result = squaredDistance < cDist;
                    break;
                case CheckType.LESS_THAN_OR_EQUAL:
                    result = squaredDistance <= cDist;
                    break;
            }
            
            if(invert) result = !result;
            return result;
        }
    }
}