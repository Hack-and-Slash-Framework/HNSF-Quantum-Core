using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionFP : HNSFStateFunction<FP>
    {
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default;
        }
    }
}