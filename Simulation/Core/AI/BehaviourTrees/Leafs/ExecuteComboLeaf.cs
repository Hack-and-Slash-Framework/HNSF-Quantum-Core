namespace Quantum
{
    [System.Serializable]
    public unsafe class ExecuteComboLeaf : BTLeaf
    {
        public AIBlackboardValueKey bbComboAssetRef;
        public AIBlackboardValueKey bbValueCurrentTarget;
        
        public override void Init(BTParams btParams, ref AIContext aiContext)
        {
            base.Init(btParams, ref aiContext);
        }

        protected override BTStatus OnUpdate(BTParams btParams, ref AIContext aiContext)
        {
            return BTStatus.Failure;
        }
    }
}