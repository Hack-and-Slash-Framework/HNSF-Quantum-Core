namespace Quantum.HFSM.Decisions
{
    [System.Serializable]
    public unsafe partial class IntComparison : HFSMDecision
    {
        public AIParamInt paramA;
        public EValueComparison comparison = EValueComparison.MoreThan;
        public AIParamInt paramB;
        
        public override bool Decide(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            var uData = ((AIContextUser*)aiContext.UserData);
            var aiConfig = frame.FindAsset(uData->HFSMAgent->Config);
            var a = paramA.Resolve(frame, entity, uData->Blackboard, aiConfig, ref aiContext);
            var b = paramB.Resolve(frame, entity, uData->Blackboard, aiConfig, ref aiContext);
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