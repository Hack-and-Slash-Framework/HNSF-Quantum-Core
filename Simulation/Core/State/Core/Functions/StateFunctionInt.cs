using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionInt : HNSFStateFunction<int>
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default;
        }
    }
}