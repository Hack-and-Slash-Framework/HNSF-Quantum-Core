namespace Quantum
{
    [System.Serializable]
    public unsafe class HitstunComparisonDecorator : BTDecorator
    {
        public override void OnEnter(BTParams btParams, ref AIContext aiContext)
        {
            base.OnEnter(btParams, ref aiContext);
        }

        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            if (!btParams.Frame.Unsafe.TryGetPointer<Hitstun>(btParams.Entity, out var hitstun)) return false;
            return hitstun->value > 0;
        }

        public override void OnExit(BTParams btParams, ref AIContext aiContext)
        {
            base.OnExit(btParams, ref aiContext);
        }
    }
}