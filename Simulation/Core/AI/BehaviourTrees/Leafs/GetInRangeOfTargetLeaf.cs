using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe class GetInRangeOfTargetLeaf : BTLeaf
    {
        public AIBlackboardValueKey targetBlackboardKey;
        public AIBlackboardValueKey paramGoToPosition;
        public AIBlackboardValueKey paramLastFrameCalculatedGoToPosition;
        public FP validRange;
        public FP timeoutAfter;
        
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