namespace Quantum
{
    [System.Serializable]
    public unsafe class EntityExistDecorator : BTDecorator
    {
        public AIBlackboardValueKey blackboardEntityRefKey;
        public bool invert;
        
        public override void OnEnter(BTParams btParams, ref AIContext aiContext)
        {
            base.OnEnter(btParams, ref aiContext);
        }

        public override bool CheckConditions(BTParams btParams, ref AIContext aiContext)
        {
            if (!btParams.Blackboard->TryGetEntityRef(btParams.Frame, blackboardEntityRefKey.Key, out var entityRef)) return false;

            var result = btParams.Frame.Exists(entityRef);
            if(invert) result = !result;
            return result;
        }

        public override void OnExit(BTParams btParams, ref AIContext aiContext)
        {
            base.OnExit(btParams, ref aiContext);
        }
    }
}