namespace Quantum
{
    [System.Serializable]
    public unsafe class BlockstunComparisonDecorator : BTDecorator
    {
        public bool invert;
        
        public override void OnEnter(BTParams btParams, ref AIContext aiContext)
        {
            base.OnEnter(btParams, ref aiContext);
        }

        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            if (!btParams.Frame.Unsafe.TryGetPointer<Blockstun>(btParams.Entity, out var blockstun)) return false;

            var result = blockstun->value > 0;
            if(invert) result = !result;
            return result;
        }

        public override void OnExit(BTParams btParams, ref AIContext aiContext)
        {
            base.OnExit(btParams, ref aiContext);
        }
    }
}