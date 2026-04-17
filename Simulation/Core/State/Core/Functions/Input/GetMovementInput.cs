using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetMovementInput : StateFunctionFPVector2
    {
        public int offset;
        
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return GetInput(frame, entity);
        }

        public FPVector2 GetInput(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<ActorInputBufferMovement>(entityRef, out var bufferMovement))
                return FPVector2.Zero;

            return bufferMovement->GetMovement(offset);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetMovementInput());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetMovementInput;
            t.offset = offset;
            return base.CopyTo(target);
        }
    }
}