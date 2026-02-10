using Quantum.Physics3D;

namespace Quantum
{
    [System.Serializable]
    public unsafe class WillBeHitThisFrameDecorator : BTDecorator
    {
        public LayerMask layerMask;
        
        private HitCollection3D hc;
        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            var f = btParams.FrameThreadSafe;
            
            if (!f.TryGetPointer<BoxCombatant>(btParams.Entity, out var boxCombatant)) return false;
            
            var hurtboxList = btParams.Frame.ResolveList(boxCombatant->hurtboxList);
            if (hurtboxList.Count == 0) return false;
            
            for (int i = 0; i < hurtboxList.Count; i++)
            {
                var hurtboxCollider = f.GetPointer<PhysicsCollider3D>(hurtboxList[i]);
                var hurtboxTransform = f.GetPointer<Transform3D>(hurtboxList[i]);

                hc = f.Physics3D.OverlapShape(*hurtboxTransform, hurtboxCollider->Shape, layerMask, QueryOptions.HitAll);

                if (hc.Count > 0) return true;
            }
            
            return false;
        }
    }
}
