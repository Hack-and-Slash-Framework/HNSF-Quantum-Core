using HnSF.core;

namespace Quantum
{
    [System.Serializable]
    public unsafe class ManageAttackTokenDecorator : BTDecorator
    {
        public AIBlackboardValueKey bbAttackTokenEntityRef;

        public override void OnExit(BTParams btParams, ref AIContext aiContext)
        {
            if (!btParams.Blackboard->TryGetEntityRef(btParams.Frame, bbAttackTokenEntityRef.Key,
                    out var attackTokenEntityRef)) return;

            if (btParams.Frame.Exists(attackTokenEntityRef)) CombatHelper.TokenHelper.ReturnToken(btParams.Frame as Frame, attackTokenEntityRef);
        }
    }
}