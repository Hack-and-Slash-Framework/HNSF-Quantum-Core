namespace Quantum
{
    [System.Serializable]
    public unsafe class AttemptAttackWithComboDecorator : BTDecorator
    {
        public AIBlackboardValueKey bbAttackTokenEntityRef;
        public AIBlackboardValueKey bbTargetEntityRef;
        public AIBlackboardValueKey bbComboAssetRef;
        
        
        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            var frame = (Frame)btParams.Frame;
            
            return false;
        }
    }
}