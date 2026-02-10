using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetNextState : StateFunctionAssetRef
    {
        public override AssetRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!stateContext.agentData->toStateRequested) return default;
            return stateContext.agentData->toState.Id;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetNextState());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetNextState;
            return base.CopyTo(target);
        }
    }
}