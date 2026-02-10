namespace Quantum
{
    [System.Serializable]
    public unsafe class RandomDecisionDecorator : BTDecorator
    {
        public int chanceOfTrue = 50;
        
        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            var frame = (Frame)btParams.Frame;
            return frame.RNG->NextInclusive(1, 100) <= chanceOfTrue;
        }
    }
}