using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionFPVector2 : HNSFStateFunction<FPVector2>
    {
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default;
        }
    }
}