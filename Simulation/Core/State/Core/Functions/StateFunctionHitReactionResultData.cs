using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionHitReactionResultData : HNSFStateFunction<DefenderHitResultData>
    {
        public override DefenderHitResultData Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default;
        }
    }
}