using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionBool : HNSFStateFunction<bool>
    {
        public override bool Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return false;
        }
    }
}