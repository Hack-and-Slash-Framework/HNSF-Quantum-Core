using HnSF.core.state;

namespace Quantum
{
    [System.Serializable]
    public unsafe class GenericStateMachineUpdateLeaf : BTLeaf
    {
        protected override BTStatus OnUpdate(BTParams btParams, ref AIContext aiContext)
        {
            HNSFStateHelper.Generic.UpdateGenericStateMachine(btParams.Frame as Frame, btParams.Entity, true);
            return BTStatus.Success;
        }
    }
}