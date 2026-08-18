namespace Quantum
{
    [System.Serializable]
    public unsafe class IntComparisonAIFunction : AIFunction<bool>
    {
        public AIParamInt paramA;
        public EValueComparison comparison = EValueComparison.MoreThan;
        public AIParamInt paramB;
        
        public override bool Execute(FrameThreadSafe frame, EntityRef entity, ref AIContext aiContext)
        {
            var uData = ((AIContextUser*)aiContext.UserData);
            var aiConfig = frame.FindAsset(uData->HFSMAgent->Config);
            var a = paramA.Resolve(frame, entity, uData->Blackboard, aiConfig);
            var b = paramB.Resolve(frame, entity, uData->Blackboard, aiConfig);
            switch (comparison)
            {
                case EValueComparison.LessThan:
                    return a < b;
                case EValueComparison.MoreThan:
                    return a > b;
                case EValueComparison.EqualTo:
                    return a == b;
                default:
                    return false;
            }
        }
    }
}
